import { useCallback } from "react";
import { useUser, useClerk } from "@clerk/clerk-react";
import { useCheckInNavigation } from "./useCheckInNavigation";
import { useCheckInData } from "./useCheckInData";
import { useCompleteCheckIn } from "./useCompleteCheckIn";
import { useShiftCheckInStore } from "@/store/shift-check-in.store";
import type { ShiftCheckInPatient } from "@/types/domain";
import type { ShiftCheckInState, ShiftCheckInActions, ShiftCheckInStep } from "../types";
import type { UnitConfig } from "@/types/domain";

type UseShiftCheckInStateParams = {
	units: Array<UnitConfig>;
};

export function useShiftCheckInState({ units }: UseShiftCheckInStateParams): ShiftCheckInState & ShiftCheckInActions {
	const { user } = useUser();
	const { signOut } = useClerk();
	const { reset: resetPersistentState } = useShiftCheckInStore();

	const navigation = useCheckInNavigation();
	const data = useCheckInData(navigation.currentStep);
	const completion = useCompleteCheckIn(user?.id ?? "", { units });

	const handleSignOut = useCallback(async (): Promise<void> => {
		return signOut().finally(() => {
			resetPersistentState();
		});
	}, [signOut, resetPersistentState]);

	const canProceedToNextStep = useCallback((): boolean => {
		switch (navigation.currentStep) {
			case 0:
				return (user?.fullName || "").trim() !== "";
			case 1:
				return data.unit !== "";
			case 2:
				return data.shift !== "";
			case 3:
				return data.patients.length > 0 && data.selectedIndexes.length > 0;
			default:
				return false;
		}
	}, [navigation.currentStep, user?.fullName, data.unit, data.shift, data.patients.length, data.selectedIndexes.length]);

	const handleNextStep = useCallback((_patients: Array<ShiftCheckInPatient>): void => {
		if (navigation.currentStep === 3 && data.selectedIndexes.length === 0) {
			data.setShowValidationError(true);
			return;
		}

		if (canProceedToNextStep()) {
			if (navigation.currentStep === 3) {
				completion.submitCheckIn({
					shiftId: data.shift,
					patients: data.patients,
					selectedIndexes: data.selectedIndexes,
					userId: user?.id ?? "",
				});
			} else {
				navigation.setCurrentStep((navigation.currentStep + 1) as ShiftCheckInStep);
			}
		}
	}, [
		navigation.currentStep,
		data.selectedIndexes,
		canProceedToNextStep,
		completion.submitCheckIn,
		data.shift,
		data.patients,
		user?.id,
		navigation.setCurrentStep,
		data.setShowValidationError,
	]);

	return {
		// State
		currentStep: navigation.currentStep,
		doctorName: user?.fullName || "",
		unit: data.unit,
		shift: data.shift,
		selectedIndexes: data.selectedIndexes,
		showValidationError: data.showValidationError,
		patients: data.patients,
		isFetching: data.isFetching,

		// Actions
		setCurrentStep: navigation.setCurrentStep,
		setUnit: data.setUnit,
		setShift: data.setShift,
		setShowValidationError: data.setShowValidationError,
		togglePatientSelection: data.togglePatientSelection,
		handleSelectAll: data.handleSelectAll,
		canProceedToNextStep,
		handleNextStep,
		handleBackStep: navigation.handleBackStep,
		handleSignOut,
	};
}
