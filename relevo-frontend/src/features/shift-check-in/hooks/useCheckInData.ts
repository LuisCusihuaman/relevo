import { useCallback, useEffect, useMemo, useState } from "react";
import { usePatientsByUnit } from "@/api";
import { formatDiagnosis } from "@/lib/formatters";
import { transformApiPatient } from "../utils/patientUtilities";
import { useShiftCheckInStore } from "@/store/shift-check-in.store";
import type { ShiftCheckInPatient } from "@/types/domain";
import type { ShiftCheckInStep } from "../types";

export function useCheckInData(currentStep: ShiftCheckInStep) {
	const {
		unit,
		shift,
		selectedIndexes,
		setState: setPersistentState,
	} = useShiftCheckInStore();

	const [showValidationError, setShowValidationError] = useState(false);

	const setUnit = useCallback((newUnit: string): void => {
		if (newUnit !== unit) {
			setPersistentState({
				unit: newUnit,
				selectedIndexes: [],
			});
			setShowValidationError(false);
		}
	}, [unit, setPersistentState]);

	const setShift = useCallback((shift: string): void => {
		setPersistentState({ shift });
	}, [setPersistentState]);

	const setSelectedIndexes = useCallback((indexes: Array<number>): void => {
		setPersistentState({ selectedIndexes: indexes });
	}, [setPersistentState]);

	// Fetch patients
	const shouldFetchPatients = currentStep >= 1 && Boolean(unit);
	const { data: apiPatients, isFetching } = usePatientsByUnit(unit, {
		enabled: shouldFetchPatients,
	});

	const patients = useMemo((): Array<ShiftCheckInPatient> => {
		if (!apiPatients || currentStep < 1 || !unit) return [];
		return apiPatients.map((p) => ({
			...transformApiPatient(p),
			diagnosis: p.diagnosis ? formatDiagnosis(p.diagnosis) : "",
		}));
	}, [apiPatients, currentStep, unit]);

	// Filter selectedIndexes to only include valid, non-assigned patients
	const validSelectedIndexes = useMemo((): Array<number> => {
		return selectedIndexes.filter((index) => {
			// Check if index is within bounds
			if (index < 0 || index >= patients.length) return false;
			// Check if patient is not assigned
			return patients[index]?.status !== "assigned";
		});
	}, [selectedIndexes, patients]);

	// Update selectedIndexes if they were filtered (only when patients change)
	useEffect(() => {
		if (patients.length > 0) {
			// Check if arrays are different
			const arraysEqual =
				validSelectedIndexes.length === selectedIndexes.length &&
				validSelectedIndexes.every((val, idx) => val === selectedIndexes[idx]);
			
			if (!arraysEqual) {
				setSelectedIndexes(validSelectedIndexes);
			}
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [patients.length, validSelectedIndexes.join(",")]);

	const togglePatientSelection = useCallback((rowIndex: number): void => {
		// Use validSelectedIndexes for consistency
		const currentValid = validSelectedIndexes;
		setSelectedIndexes(
			currentValid.includes(rowIndex)
				? currentValid.filter((index: number) => index !== rowIndex)
				: [...currentValid, rowIndex]
		);
		if (showValidationError) setShowValidationError(false);
	}, [validSelectedIndexes, showValidationError, setSelectedIndexes]);

	const handleSelectAll = useCallback((patients: Array<ShiftCheckInPatient>): void => {
		// Only select patients that are not assigned
		const selectableIndexes = patients
			.map((patient, index) => (patient.status !== "assigned" ? index : null))
			.filter((index): index is number => index !== null);
		
		// Use validSelectedIndexes for consistency
		const currentValid = validSelectedIndexes;
		const allSelectableSelected = selectableIndexes.every((index) =>
			currentValid.includes(index)
		);

		if (allSelectableSelected && selectableIndexes.length > 0) {
			// Deselect all selectable patients
			setSelectedIndexes(
				currentValid.filter((index) => !selectableIndexes.includes(index))
			);
		} else {
			// Select all selectable patients (excluding assigned ones)
			setSelectedIndexes([
				...currentValid.filter((index) => !selectableIndexes.includes(index)),
				...selectableIndexes,
			]);
		}
		if (showValidationError) setShowValidationError(false);
	}, [validSelectedIndexes, showValidationError, setSelectedIndexes]);

	// Count assigned patients
	const assignedPatientsCount = useMemo((): number => {
		return patients.filter((patient) => patient.status === "assigned").length;
	}, [patients]);

	return {
		unit,
		shift,
		selectedIndexes: validSelectedIndexes,
		patients,
		assignedPatientsCount,
		isFetching,
		showValidationError,
		setShowValidationError,
		setUnit,
		setShift,
		togglePatientSelection,
		handleSelectAll,
	};
}
