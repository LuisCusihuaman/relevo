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
		if (selectedIndexes.length === patients.length) {
			setSelectedIndexes([]);
		} else {
			setSelectedIndexes(Array.from({ length: patients.length }, (_, index) => index));
		}
		if (showValidationError) setShowValidationError(false);
	}, [selectedIndexes.length, showValidationError, setSelectedIndexes]);

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
