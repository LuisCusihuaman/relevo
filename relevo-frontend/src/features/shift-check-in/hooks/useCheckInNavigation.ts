import { useCallback } from "react";
import { useShiftCheckInStore } from "@/store/shift-check-in.store";
import type { ShiftCheckInStep } from "../types";

export function useCheckInNavigation() {
	const {
		currentStep,
		setState: setPersistentState,
	} = useShiftCheckInStore();

	const setCurrentStep = useCallback((step: ShiftCheckInStep): void => {
		setPersistentState({ currentStep: step });
	}, [setPersistentState]);

	// Safeguard: Prevent currentStep from resetting to 0 once user has progressed
	const stableSetCurrentStep = useCallback((stepUpdater: ShiftCheckInStep | ((previous: ShiftCheckInStep) => ShiftCheckInStep)): void => {
		const newStep = typeof stepUpdater === 'function' ? stepUpdater(currentStep) : stepUpdater;
		if (newStep === 0 && currentStep > 0) {
			return;
		}
		setCurrentStep(newStep);
	}, [currentStep, setCurrentStep]);

	const handleBackStep = useCallback((): void => {
		if (currentStep > 0) {
			stableSetCurrentStep((previous) => (previous - 1) as ShiftCheckInStep);
		}
	}, [currentStep, stableSetCurrentStep]);

	return {
		currentStep,
		setCurrentStep: stableSetCurrentStep,
		handleBackStep,
	};
}
