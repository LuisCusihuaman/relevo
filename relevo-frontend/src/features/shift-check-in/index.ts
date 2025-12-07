// Main components
export { ShiftCheckInWizard } from "./components/ShiftCheckInWizard";

// Individual step components
export { DoctorInfoStep } from "./components/DoctorInfoStep";
export { UnitSelectionStep } from "./components/UnitSelectionStep";
export { ShiftSelectionStep } from "./components/ShiftSelectionStep";
export { PatientSelectionStep } from "./components/PatientSelectionStep";

// Shared components
export { ShiftCheckInHeader } from "./components/ShiftCheckInHeader";
export { ShiftCheckInNavigation } from "./components/ShiftCheckInNavigation";

// Hooks
export { useShiftCheckInState } from "./hooks/useShiftCheckInState";

// Types
export type {
	ShiftCheckInStatus,
	UnitConfig,
	ShiftConfig,
	ShiftCheckInPatient,
	ShiftCheckInStep,
	ShiftCheckInState,
	ShiftCheckInActions,
} from "./types";

export type SeverityLevel = IllnessSeverity;
import type { IllnessSeverity } from "@/types/domain";
