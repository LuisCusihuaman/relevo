import { useCallback, useMemo, useState } from "react";
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

	const togglePatientSelection = useCallback((rowIndex: number): void => {
		setSelectedIndexes(
			selectedIndexes.includes(rowIndex)
				? selectedIndexes.filter((index: number) => index !== rowIndex)
				: [...selectedIndexes, rowIndex]
		);
		if (showValidationError) setShowValidationError(false);
	}, [selectedIndexes, showValidationError, setSelectedIndexes]);

	const handleSelectAll = useCallback((patients: Array<ShiftCheckInPatient>): void => {
		// Only select patients that are not assigned
		const selectableIndexes = patients
			.map((patient, index) => (patient.status !== "assigned" ? index : null))
			.filter((index): index is number => index !== null);
		
		const allSelectableSelected = selectableIndexes.every((index) =>
			selectedIndexes.includes(index)
		);

		if (allSelectableSelected && selectableIndexes.length > 0) {
			// Deselect all selectable patients
			setSelectedIndexes(
				selectedIndexes.filter((index) => !selectableIndexes.includes(index))
			);
		} else {
			// Select all selectable patients (excluding assigned ones)
			setSelectedIndexes([
				...selectedIndexes.filter((index) => !selectableIndexes.includes(index)),
				...selectableIndexes,
			]);
		}
		if (showValidationError) setShowValidationError(false);
	}, [selectedIndexes, showValidationError, setSelectedIndexes]);

	return {
		unit,
		shift,
		selectedIndexes,
		patients,
		isFetching,
		showValidationError,
		setShowValidationError,
		setUnit,
		setShift,
		togglePatientSelection,
		handleSelectAll,
	};
}
