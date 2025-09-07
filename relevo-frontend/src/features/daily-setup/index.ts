// Main components
export { SetupWizard } from "./components/SetupWizard";

// Individual step components
export { DoctorInfoStep } from "./components/DoctorInfoStep";
export { UnitSelectionStep } from "./components/UnitSelectionStep";
export { ShiftSelectionStep } from "./components/ShiftSelectionStep";
export { PatientSelectionStep } from "./components/PatientSelectionStep";

// Shared components
export { SetupHeader } from "./components/SetupHeader";
export { SetupNavigation } from "./components/SetupNavigation";

// Hooks
export { useSetupState } from "./hooks/useSetupState";

// Types
export type {
	SetupStatus,
	SeverityLevel,
	UnitConfig,
	ShiftConfig,
	SetupPatient,
	SetupStep,
	SetupState,
	SetupActions,
} from "./types";
