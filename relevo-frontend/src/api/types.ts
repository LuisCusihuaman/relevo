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

export type PatientSummary = {
	id: string;
	patientId: string;
	physicianId: string;
	summaryText: string;
	createdAt: string;
	updatedAt: string;
	lastEditedBy: string;
};

export type PatientSummaryResponse = {
	summary: PatientSummary | null;
};

export type CreatePatientSummaryRequest = {
	summaryText: string;
};

export type UpdatePatientSummaryRequest = {
	summaryText: string;
};

export type PatientSummaryUpdateResponse = {
	success: boolean;
	message: string;
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
	status: string;
	illnessSeverity: HandoverIllnessSeverity;
	patientSummary: HandoverPatientSummary;
	actionItems: Array<HandoverActionItem>;
	situationAwarenessDocId?: string;
	synthesis?: HandoverSynthesis;
	shiftName: string;
	createdBy: string;
	assignedTo: string;
	createdByName?: string;
	assignedToName?: string;
	receiverUserId?: string;
	createdAt?: string; // Date when handover was created
	readyAt?: string;
	startedAt?: string;
	acknowledgedAt?: string;
	acceptedAt?: string;
	completedAt?: string;
	cancelledAt?: string;
	rejectedAt?: string;
	rejectionReason?: string;
	expiredAt?: string;
	handoverType?: "ShiftToShift" | "TemporaryCoverage" | "Consult";
	handoverWindowDate?: string;
	fromShiftId?: string;
	toShiftId?: string;
	toDoctorId?: string;
	stateName: "Draft" | "Ready" | "InProgress" | "Accepted" | "Completed" | "Cancelled" | "Rejected" | "Expired";
};

export type PaginatedHandovers = {
	pagination: PaginationInfo;
	items: Array<Handover>;
};

// User types
export type User = {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	fullName: string;
	roles: Array<string>;
	isActive: boolean;
};

// Handover types
export type HandoverParticipant = {
	id: string;
	userId: string;
	userName: string;
	userRole?: string;
	status: "active" | "inactive" | "viewing";
	joinedAt: string;
	lastActivity: string;
};

export type HandoverSection = {
	id: string;
	sectionType: "illness_severity" | "patient_summary" | "action_items" | "situation_awareness" | "synthesis";
	content?: string;
	status: "draft" | "in_progress" | "completed";
	lastEditedBy?: string;
	createdAt: string;
	updatedAt: string;
};

export type HandoverSyncStatus = {
	id: string;
	syncStatus: "synced" | "syncing" | "pending" | "offline" | "error";
	lastSync: string;
	version: number;
};

export type HandoverMessage = {
	id: string;
	handoverId: string;
	userId: string;
	userName: string;
	messageText: string;
	messageType: "message" | "system" | "notification";
	createdAt: string;
	updatedAt: string;
};

export type HandoverActivityItem = {
	id: string;
	handoverId: string;
	userId: string;
	userName: string;
	activityType: string;
	activityDescription?: string;
	sectionAffected?: string;
	metadata?: Record<string, any>;
	createdAt: string;
};

export type HandoverChecklistItem = {
	id: string;
	handoverId: string;
	userId: string;
	itemId: string;
	itemCategory: string;
	itemLabel: string;
	itemDescription?: string;
	isRequired: boolean;
	isChecked: boolean;
	checkedAt?: string;
	createdAt: string;
};

export type HandoverContingencyPlan = {
	id: string;
	handoverId: string;
	conditionText: string;
	actionText: string;
	priority: "low" | "medium" | "high";
	status: "active" | "planned" | "completed";
	createdBy: string;
	createdAt: string;
	updatedAt: string;
};

export type ActiveHandoverData = {
	handover: Handover;
	participants: Array<HandoverParticipant>;
	sections: Array<HandoverSection>;
	syncStatus?: HandoverSyncStatus;
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
