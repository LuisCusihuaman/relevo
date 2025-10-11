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
	patientName?: string; // For display in lists/cards - full patient data comes from /patient endpoint
	status: string;
	illnessSeverity: HandoverIllnessSeverity;
	patientSummary: HandoverPatientSummary;
	situationAwarenessDocId?: string;
	synthesis?: HandoverSynthesis;
	shiftName: string;
	createdBy: string;
	assignedTo: string;
	receiverUserId?: string;
	responsiblePhysicianId: string;
	responsiblePhysicianName: string;
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

export type SituationAwarenessSection = {
	id: string;
	handoverId: string;
	sectionType: string;
	content: string | null;
	status: string;
	lastEditedBy: string | null;
	createdAt: string;
	updatedAt: string;
};

export type SituationAwarenessResponse = {
	section: SituationAwarenessSection | null;
};

export type ContingencyPlansResponse = {
	plans: HandoverContingencyPlan[];
};

export type PatientDataDto = {
    handoverId: string;
    illnessSeverity: string;
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
    illnessSeverity: string;
    summaryText?: string;
    status: string;
};

export type SynthesisDto = {
    handoverId: string;
    content?: string;
    status: string;
    lastEditedBy?: string;
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
};

export type ApiResponse = {
	success: boolean;
	message: string;
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

// Patient Handover Data - Complete patient data for handover (consolidated from /patient and /patient-data)
export type PatientHandoverData = {
	id: string;
	name: string;
	dob: string; // Raw DOB, frontend calculates age
	mrn: string;
	admissionDate: string;
	currentDateTime: string;
	primaryTeam: string;
	primaryDiagnosis: string;
	room: string;
	unit: string;
	assignedPhysician: {
		name: string;
		role: string;
		color: string;
		shiftEnd?: string;
		shiftStart?: string;
		status: string;
		patientAssignment: string;
	} | null;
	receivingPhysician: {
		name: string;
		role: string;
		color: string;
		shiftEnd?: string;
		shiftStart?: string;
		status: string;
		patientAssignment: string;
	} | null;
	// Medical status data (consolidated from /patient-data endpoint)
	illnessSeverity?: string;
	summaryText?: string;
	lastEditedBy?: string;
	updatedAt?: string;
};
