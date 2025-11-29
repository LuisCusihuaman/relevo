export type ShiftCheckInStatus = "pending" | "in-progress" | "complete";
export type SeverityLevel = "stable" | "watcher" | "unstable";

export type UnitConfig = {
	id: string;
	name: string;
	description: string;
};

export type ShiftConfig = {
	id: string;
	name: string;
	time: string;
};

export type ShiftCheckInPatient = {
	id: string | number;
	name: string;
	age?: number;
	room: string;
	diagnosis: string;
	status: ShiftCheckInStatus;
	severity: SeverityLevel;
};

export type ShiftCheckInStep = 0 | 1 | 2 | 3;

export type ShiftCheckInState = {
	currentStep: ShiftCheckInStep;
	isMobile: boolean;
	doctorName: string; // Derived from Clerk user
	unit: string;
	shift: string;
	selectedIndexes: Array<number>;
	showValidationError: boolean;
	isEditing: boolean;
	patients: Array<ShiftCheckInPatient>;
	isFetching: boolean;
};

export type ShiftCheckInActions = {
	setCurrentStep: (step: ShiftCheckInStep) => void;
	setUnit: (unit: string) => void;
	setShift: (shift: string) => void;
	setSelectedIndexes: (indexes: Array<number>) => void;
	setShowValidationError: (show: boolean) => void;
	togglePatientSelection: (index: number) => void;
	handleSelectAll: (patients: Array<ShiftCheckInPatient>) => void;
	canProceedToNextStep: () => boolean;
	handleNextStep: (patients: Array<ShiftCheckInPatient>) => void;
	handleBackStep: () => void;
	handleSignOut: () => Promise<void>;
};
