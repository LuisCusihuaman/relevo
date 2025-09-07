// Patient API exports
export * from "./patients";

// Common types
export type { SetupPatient, Patient } from "@/common/types";

// Re-export common types for convenience
export type {
	PatientSummaryCard,
	PatientDetail,
	PatientHandoverTimelineItem,
	PaginationInfo,
	PaginatedPatientSummaryCards,
	PaginatedPatientHandoverTimeline,
	Unit,
	Shift,
	AssignPatientsPayload,
} from "./patients";
