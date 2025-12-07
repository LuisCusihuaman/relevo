import type {
	ShiftCheckInPatient,
	IllnessSeverity,
	HandoverStatus,
	Patient,
	SituationAwarenessStatus,
	UserRole,
} from "@/types/domain";

// API Response Types (matching the OpenAPI schema)
export type PatientSummaryCard = {
	id: string;
	name: string;
	handoverStatus: "NotStarted" | "Active" | "InProgress" | "Completed";
	handoverId: string | null;
};

// Extends Patient but enforces certain fields that are optional in Patient but required in API
export interface PatientDetail extends Patient {
	id: string;
	name: string;
	mrn: string;
	dob: string; // Enforce string format from API
	gender: "Male" | "Female" | "Other" | "Unknown";
	admissionDate: string;
	currentUnit: string; // Maps to 'unit' in Patient but named different in API? Check mapper.
	roomNumber: string; // Maps to 'room'
	diagnosis: string;
	allergies: Array<string>;
	medications: Array<string>;
	notes: string;
}

export type PatientHandoverTimelineItem = {
	handoverId: string;
	status: "Active" | "InProgress" | "Completed";
	createdAt: string;
	completedAt: string | null;
	shiftName: string;
	illnessSeverity: IllnessSeverity;
};

export type PaginationInfo = {
	totalItems: number;
	totalPages: number;
	currentPage: number;
	pageSize: number;
};

export type PaginatedResponse<T> = {
	pagination: PaginationInfo;
	items: Array<T>;
};

export type PaginatedPatientSummaryCards = PaginatedResponse<PatientSummaryCard>;

export type PaginatedPatientHandoverTimeline =
	PaginatedResponse<PatientHandoverTimelineItem>;

export type PatientSummary = {
	id: string;
	patientId: string;
	physicianId: string;
	summaryText: string;
	createdAt: string;
	updatedAt: string;
	lastEditedBy: string;
};

export type ApiResponse<T> = {
	success: boolean;
	message: string;
	data?: T;
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

export type PatientSummaryUpdateResponse = ApiResponse<void>;

// Handover types (matching the OpenAPI schema)
export type HandoverActionItem = {
	id: string;
	handoverId: string;
	description: string;
	isCompleted: boolean;
	createdAt: string;
	updatedAt: string;
	completedAt: string | null;
};

export type GetHandoverActionItemsResponse = {
	actionItems: Array<HandoverActionItem>;
};

// Wrapper types kept for API compatibility, but using Domain types
export type HandoverIllnessSeverity = {
	severity: IllnessSeverity;
};

export type HandoverPatientSummary = {
	content: string;
};

export type HandoverSynthesis = {
	content: string;
};

export type HandoverSummary = {
	id: string;
	patientId: string;
	shiftName: string;
	stateName: HandoverStatus;
	illnessSeverity: HandoverIllnessSeverity;
	createdBy: string;
	createdAt?: string;
	assignedTo: string;
	responsiblePhysicianName: string;
	handoverType?: "ShiftToShift" | "TemporaryCoverage" | "Consult";
	updatedAt?: string;
};

export type HandoverDetail = {
	id: string;
	assignmentId: string;
	patientId: string;
	illnessSeverity: HandoverIllnessSeverity;
	patientSummary: HandoverPatientSummary;
	shiftName: string;
	createdBy: string;
	assignedTo: string;
	receiverUserId?: string;
	responsiblePhysicianId: string;
	responsiblePhysicianName: string;
	createdAt?: string;
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
	stateName: HandoverStatus;
	// V3 Fields
	shiftWindowId?: string;
	previousHandoverId?: string;
	senderUserId?: string;
	readyByUserId?: string;
	startedByUserId?: string;
	completedByUserId?: string;
	cancelledByUserId?: string;
	cancelReason?: string;
};

export type Handover = HandoverDetail; // Deprecated: Use HandoverDetail or HandoverSummary

export type PaginatedHandovers = PaginatedResponse<HandoverSummary>;

// User types
export type User = {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	fullName: string;
	roles: Array<UserRole>;
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
	metadata?: Record<string, unknown>;
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

export type SituationAwarenessDto = {
	handoverId: string;
	content: string | null;
	status: SituationAwarenessStatus;
	lastEditedBy: string;
	updatedAt: string;
};

export type SituationAwarenessResponse = {
	situationAwareness: SituationAwarenessDto | null;
};

export type ContingencyPlansResponse = {
	contingencyPlans: Array<HandoverContingencyPlan>;
};

export type PatientDataDto = {
	handoverId: string;
	illnessSeverity: IllnessSeverity; // Ideally just IllnessSeverity, but keeping string for safety if API sends invalid
	summaryText?: string;
	lastEditedBy?: string;
	status: string;
	createdAt: string;
	updatedAt: string;
};

export type PatientDataResponse = {
	patientData: PatientDataDto | null;
};

export type UpdatePatientDataRequest = {
	illnessSeverity: IllnessSeverity;
	summaryText?: string;
};

export type SynthesisDto = {
	handoverId: string;
	content?: string;
	status: string;
	lastEditedBy: string;
	updatedAt: string;
};

export type SynthesisResponse = {
	synthesis: SynthesisDto | null;
};

export type CreateContingencyPlanRequest = {
	conditionText: string;
	actionText: string;
	priority: "low" | "medium" | "high";
};

export type UpdateSituationAwarenessRequest = {
	content: string;
	status: SituationAwarenessStatus;
};

// Additional types for Shift Check-In
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

export type ShiftCheckInPatientsResponse = {
	patients?: Array<ShiftCheckInPatient>;
	Patients?: Array<ShiftCheckInPatient>;
};

// Consolidated types to avoid God Object
export type PhysicianAssignment = {
	name: string;
	role: string;
	color: string;
	shiftEnd?: string;
	shiftStart?: string;
	status: string;
	patientAssignment: string;
};

// Patient Handover Data - Complete patient data for handover
export interface PatientHandoverData extends Patient {
	id: string;
	// Inherits name, mrn, room, unit, diagnosis (primaryDiagnosis in API?) from Patient
	// We need to map if names differ
	currentDateTime: string;
	primaryTeam: string;
	primaryDiagnosis: string; // Map to diagnosis?
	assignedPhysician: PhysicianAssignment | null;
	receivingPhysician: PhysicianAssignment | null;
	// Medical status data
	summaryText?: string;
	lastEditedBy?: string;
	updatedAt?: string;
}
