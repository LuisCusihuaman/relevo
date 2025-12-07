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

// Types - Store types from local, Domain types from @/types/domain
export type { ShiftCheckInStep, ShiftCheckInState, ShiftCheckInActions } from "./types";
export type {
	ShiftCheckInPatient,
	ShiftCheckInStatus,
	UnitConfig,
	ShiftConfig,
	IllnessSeverity as SeverityLevel,
} from "@/types/domain";
