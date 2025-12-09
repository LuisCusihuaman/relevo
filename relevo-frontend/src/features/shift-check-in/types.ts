/**
 * Shift Check-In Store Types
 * Rule: Zustand-CleanUp - Store-specific types only
 *
 * Domain types: import from @/types/domain
 */
import type { ShiftCheckInPatient } from "@/types/domain";

export type ShiftCheckInStep = 0 | 1 | 2 | 3;

export type ShiftCheckInState = {
	currentStep: ShiftCheckInStep;
	doctorName: string;
	unit: string;
	shift: string;
	selectedIndexes: Array<number>;
	showValidationError: boolean;
	patients: Array<ShiftCheckInPatient>;
	isFetching: boolean;
};

export type ShiftCheckInActions = {
	setCurrentStep: (step: ShiftCheckInStep) => void;
	setUnit: (unit: string) => void;
	setShift: (shift: string) => void;
	setShowValidationError: (show: boolean) => void;
	togglePatientSelection: (index: number) => void;
	handleSelectAll: (patients: Array<ShiftCheckInPatient>) => void;
	canProceedToNextStep: () => boolean;
	handleNextStep: (patients: Array<ShiftCheckInPatient>) => void;
	handleBackStep: () => void;
	handleSignOut: () => Promise<void>;
};
