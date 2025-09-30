import { useEffect, useState, useCallback, useMemo } from "react";
import { useUser, useClerk } from "@clerk/clerk-react";
import { useNavigate } from "@tanstack/react-router";
import { useAssignPatients, usePatientsByUnit, useReadyHandover, usePendingHandovers } from "@/api";
import { formatDiagnosis } from "@/lib/formatters";
import { transformApiPatient } from "../utils/patientUtilities";
import type { SetupState, SetupActions, SetupPatient, SetupStep } from "../types";
import { useDailySetupStore } from "@/store/daily-setup.store";

export function useSetupState(): SetupState & SetupActions {
	const navigate = useNavigate();
	const { user } = useUser();
	const { signOut } = useClerk();
	const assignMutation = useAssignPatients();
	const readyHandoverMutation = useReadyHandover();

	// Remainder of the hook...
	const { data: pendingHandovers } = usePendingHandovers(user?.id ?? "");

	// Zustand state
	const {
		currentStep,
		doctorName,
		unit,
		shift,
		selectedIndexes,
		setState: setPersistentState,
		reset: resetPersistentState,
	} = useDailySetupStore();

	// Local UI state
	const [isMobile, setIsMobile] = useState(false);
	const [showValidationError, setShowValidationError] = useState(false);
	const isEditing = false;

	const setCurrentStep = useCallback((step: SetupStep): void => { setPersistentState({ currentStep: step }); }, [setPersistentState]);
	const setDoctorName = useCallback((name: string): void => { setPersistentState({ doctorName: name }); }, [setPersistentState]);
	
	const setUnit = useCallback((newUnit: string): void => {
		// Only reset selections if the unit is actually changing to a new value
		if (newUnit !== unit) {
			setPersistentState({
				unit: newUnit,
				selectedIndexes: [], // Atomically reset selections with unit change
			});
			setShowValidationError(false);
		}
	}, [unit, setPersistentState]);

	const setShift = useCallback((shift: string): void => { setPersistentState({ shift }); }, [setPersistentState]);
	const setSelectedIndexes = useCallback((indexes: Array<number>): void => { setPersistentState({ selectedIndexes: indexes }); }, [setPersistentState]);

	// Mobile detection handler
	const checkIsMobile = useCallback((): void => {
		setIsMobile(window.innerWidth < 768);
	}, []);
	
	// Safeguard: Prevent currentStep from resetting to 0 once user has progressed
	const stableSetCurrentStep = useCallback((stepUpdater: SetupStep | ((previous: SetupStep) => SetupStep)): void => {
		const newStep = typeof stepUpdater === 'function' ? stepUpdater(currentStep) : stepUpdater;
		if (newStep === 0 && currentStep > 0) {
			// Do not allow going back to step 0
			return;
		}
		setCurrentStep(newStep);
	}, [currentStep, setCurrentStep]);

	// Fetch patients based on selected unit - only fetch when unit is selected (step 1+)
	const shouldFetchPatients = currentStep >= 1 && Boolean(unit);

	// Always call the hook with the current unit (stable), but control enabled condition
	const { data: apiPatients, isFetching } = usePatientsByUnit(unit, {
		enabled: shouldFetchPatients, // Control when to fetch based on step and unit selection
	});

	// Transform patients data
	const patients = useMemo((): Array<SetupPatient> => {
		if (!apiPatients || currentStep < 1 || !unit) return [];

		return apiPatients.map((p) => ({
			...transformApiPatient(p),
			diagnosis: p.diagnosis ? formatDiagnosis(p.diagnosis) : "",
		}));
	}, [apiPatients, currentStep, unit]);

	// Mobile detection
	useEffect((): (() => void) => {
		checkIsMobile();
		window.addEventListener("resize", checkIsMobile);
		return (): void => {
			window.removeEventListener("resize", checkIsMobile);
		};
	}, [checkIsMobile]);


	const togglePatientSelection = useCallback((rowIndex: number): void => {
		setSelectedIndexes(
			selectedIndexes.includes(rowIndex)
				? selectedIndexes.filter((index: number) => index !== rowIndex)
				: [...selectedIndexes, rowIndex]
		);
		if (showValidationError) setShowValidationError(false);
	}, [selectedIndexes, showValidationError, setSelectedIndexes]);

	const handleSelectAll = useCallback((patients: Array<SetupPatient>): void => {
		if (selectedIndexes.length === patients.length) {
			setSelectedIndexes([]);
		} else {
			setSelectedIndexes(Array.from({ length: patients.length }, (_, index) => index));
		}
		if (showValidationError) setShowValidationError(false);
	}, [selectedIndexes.length, showValidationError, setSelectedIndexes]);

	const canProceedToNextStep = useCallback((): boolean => {
		switch (currentStep) {
			case 0:
				return doctorName.trim() !== "";
			case 1:
				return unit !== "";
			case 2:
				return shift !== "";
			case 3:
				return patients.length > 0 && selectedIndexes.length > 0;
			default:
				return false;
		}
	}, [currentStep, doctorName, unit, shift, patients.length, selectedIndexes.length]);

	const handleNextStep = useCallback((patients: Array<SetupPatient>): void => {
		if (currentStep === 3 && selectedIndexes.length === 0) {
			setShowValidationError(true);
			return;
		}

		if (canProceedToNextStep()) {
			if (currentStep === 3) {
				const shiftId = shift;
				const selectedPatientIds = selectedIndexes
					.map((index) => patients[index]?.id)
					.filter((id): id is string => Boolean(id))
					.map((id) => String(id));

				const payload = { shiftId, patientIds: selectedPatientIds };
				assignMutation.mutate(payload, {
					onSuccess: () => {
						// After assignment, find the draft handovers for the selected patients and mark them as ready
						if (pendingHandovers?.handovers) {
							const draftHandovers = pendingHandovers.handovers.filter(h => 
								selectedPatientIds.includes(h.patientId) && h.stateName === "Draft"
							);
							draftHandovers.forEach(h => {
								readyHandoverMutation.mutate(h.id);
							});
						}

						window.localStorage.setItem("dailySetupCompleted", "true");
						resetPersistentState(); // Reset zustand store on success
						void navigate({ to: "/" });
					},
					onError: (error) => {
						console.error('Assignment failed:', error);
					},
				});
			} else {
				stableSetCurrentStep((previous) => (previous + 1) as SetupStep);
			}
		}
	}, [currentStep, selectedIndexes, canProceedToNextStep, assignMutation, navigate, resetPersistentState, shift, stableSetCurrentStep, pendingHandovers, readyHandoverMutation]);

	const handleBackStep = useCallback((): void => {
		if (currentStep > 0) {
			stableSetCurrentStep((previous) => (previous - 1) as SetupStep);
			setShowValidationError(false);
		}
	}, [currentStep, stableSetCurrentStep]);

	const handleSignOut = useCallback(async (): Promise<void> => {
		return signOut().finally(() => {
			resetPersistentState();
		});
	}, [signOut, resetPersistentState]);

	return {
		// State
		currentStep,
		isMobile,
		doctorName,
		unit,
		shift,
		selectedIndexes,
		showValidationError,
		isEditing,
		patients,
		isFetching,

		// Actions
		setCurrentStep: stableSetCurrentStep,
		setDoctorName,
		setUnit,
		setShift,
		setSelectedIndexes,
		setShowValidationError,
		togglePatientSelection,
		handleSelectAll,
		canProceedToNextStep,
		handleNextStep,
		handleBackStep,
		handleSignOut,
	};
}
