import type { SetupPatient } from "@/common/types";

// API Response Types (matching the OpenAPI schema)
export type PatientSummaryCard = {
	id: string;
	name: string;
	handoverStatus: "NotStarted" | "Active" | "InProgress" | "Completed";
	handoverId: string | null;
};

export type PatientDetail = {
	id: string;
	name: string;
	mrn: string;
	dob: string;
	gender: "Male" | "Female" | "Other" | "Unknown";
	admissionDate: string;
	currentUnit: string;
	roomNumber: string;
	diagnosis: string;
	allergies: Array<string>;
	medications: Array<string>;
	notes: string;
};

export type PatientHandoverTimelineItem = {
	handoverId: string;
	status: "Active" | "InProgress" | "Completed";
	createdAt: string;
	completedAt: string | null;
	shiftName: string;
	illnessSeverity: "Stable" | "Watcher" | "Unstable";
};

export type PaginationInfo = {
	totalItems: number;
	totalPages: number;
	currentPage: number;
	pageSize: number;
};

export type PaginatedPatientSummaryCards = {
	pagination: PaginationInfo;
	items: Array<PatientSummaryCard>;
};

export type PaginatedPatientHandoverTimeline = {
	pagination: PaginationInfo;
	items: Array<PatientHandoverTimelineItem>;
};

// Handover types (matching the OpenAPI schema)
export type HandoverActionItem = {
	id: string;
	description: string;
	isCompleted: boolean;
};

export type HandoverIllnessSeverity = {
	severity: "Stable" | "Watcher" | "Unstable";
};

export type HandoverPatientSummary = {
	content: string;
};

export type HandoverSynthesis = {
	content: string;
};

export type Handover = {
	id: string;
	assignmentId: string;
	patientId: string;
	patientName?: string;
	status: "Active" | "InProgress" | "Completed";
	illnessSeverity: HandoverIllnessSeverity;
	patientSummary: HandoverPatientSummary;
	actionItems: Array<HandoverActionItem>;
	situationAwarenessDocId?: string;
	synthesis?: HandoverSynthesis;
	shiftName: string;
	createdBy: string;
	assignedTo: string;
	createdAt?: string; // Date when handover was created
};

export type PaginatedHandovers = {
	pagination: PaginationInfo;
	items: Array<Handover>;
};

// Additional types for Daily Setup
export type Unit = {
	id: string;
	name: string;
	description?: string;
};

export type Shift = {
	id: string;
	name: string;
	startTime?: string;
	endTime?: string;
};

export type AssignPatientsPayload = {
	shiftId: string;
	patientIds: Array<string>;
};

// Internal response types for API parsing
export type UnitsResponse = {
	units?: Array<Unit>;
	Units?: Array<Unit>;
};

export type ShiftsResponse = {
	shifts?: Array<Shift>;
	Shifts?: Array<Shift>;
};

export type SetupPatientsResponse = {
	patients?: Array<SetupPatient>;
	Patients?: Array<SetupPatient>;
};
