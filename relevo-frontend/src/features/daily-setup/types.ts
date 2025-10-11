export type SetupStatus = "pending" | "in-progress" | "complete";
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

export type SetupPatient = {
	id: string | number;
	name: string;
	age?: number;
	room: string;
	diagnosis: string;
	status: SetupStatus;
	severity: SeverityLevel;
};

export type SetupStep = 0 | 1 | 2 | 3;

export type SetupState = {
	currentStep: SetupStep;
	isMobile: boolean;
	doctorName: string; // Derived from Clerk user
	unit: string;
	shift: string;
	selectedIndexes: Array<number>;
	showValidationError: boolean;
	isEditing: boolean;
	patients: Array<SetupPatient>;
	isFetching: boolean;
};

export type SetupActions = {
	setCurrentStep: (step: SetupStep) => void;
	setUnit: (unit: string) => void;
	setShift: (shift: string) => void;
	setSelectedIndexes: (indexes: Array<number>) => void;
	setShowValidationError: (show: boolean) => void;
	togglePatientSelection: (index: number) => void;
	handleSelectAll: (patients: Array<SetupPatient>) => void;
	canProceedToNextStep: () => boolean;
	handleNextStep: (patients: Array<SetupPatient>) => void;
	handleBackStep: () => void;
	handleSignOut: () => Promise<void>;
};
