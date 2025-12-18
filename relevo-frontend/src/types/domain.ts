/**
 * Domain Types - Single Source of Truth for UI types
 *
 * Rule: Strict-TS - All types used by the UI after API mapping
 *
 * Architecture:
 *   OpenAPI Schema (SSOT) → Mappers → Domain Types → Views
 *
 * These types represent the data as the UI consumes it, not as the API returns it.
 * For API types, use @/api/generated
 */

// =============================================================================
// Enums & Literals
// =============================================================================

export type IllnessSeverity = "stable" | "watcher" | "unstable" | "critical";

export type HandoverStatus =
	| "Draft"
	| "Ready"
	| "InProgress"
	| "Completed"
	| "Cancelled";

export type ShiftCheckInStatus = "pending" | "assigned" | "in-progress" | "complete";

export type SituationAwarenessStatus = "Draft" | "Ready" | "InProgress" | "Completed";

export type UserRole = "physician" | "nurse" | "admin" | "student" | (string & {});

export type Priority = "low" | "medium" | "high";

export type ContingencyStatus = "active" | "planned" | "completed";

// =============================================================================
// Patient Types
// =============================================================================

export type Patient = {
	id: string;
	name: string;
	mrn: string;
	dob?: string;
	age?: number;
	gender?: "Male" | "Female" | "Other" | "Unknown";
	admissionDate?: string;
	weight?: string | number;
	height?: string | number;
	room?: string;
	bed?: string;
	unit?: string;
	diagnosis?: string;
	illnessSeverity?: IllnessSeverity;
	primaryTeam?: string;
	allergies?: Array<string>;
	medications?: Array<string>;
	notes?: string;
};

export type PatientSummaryCard = {
	id: string;
	name: string;
	handoverStatus: string;
	handoverId: string | null;
	severity?: string | null;
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

export type PhysicianAssignment = {
	name: string;
	role: string;
	color: string;
	shiftEnd?: string;
	shiftStart?: string;
	status: string;
	patientAssignment: string;
};

export type PatientHandoverData = Patient & {
	currentDateTime: string;
	primaryTeam: string;
	primaryDiagnosis: string;
	assignedPhysician: PhysicianAssignment | null;
	receivingPhysician: PhysicianAssignment | null;
	summaryText?: string;
	lastEditedBy?: string;
	updatedAt?: string;
};

// =============================================================================
// Handover Types
// =============================================================================

export type HandoverSummary = {
	id: string;
	patientId: string;
	patientName: string | null;
	shiftName: string;
	stateName: HandoverStatus;
	illnessSeverity: IllnessSeverity;
	createdBy: string;
	createdByName: string | null;
	assignedTo: string;
	assignedToName: string | null;
	responsiblePhysicianName: string;
	createdAt?: string;
	completedAt?: string;
};

export type HandoverDetail = {
	id: string;
	patientId: string;
	patientName: string | null;
	shiftName: string;
	stateName: HandoverStatus;
	illnessSeverity: IllnessSeverity;
	patientSummaryContent: string;
	synthesisContent: string | null;
	responsiblePhysicianId: string;
	responsiblePhysicianName: string;
	createdBy: string;
	assignedTo: string;
	receiverUserId?: string;
	createdAt?: string;
	readyAt?: string;
	startedAt?: string;
	completedAt?: string;
	cancelledAt?: string;
	version: number;
	shiftWindowId?: string;
	previousHandoverId?: string;
	cancelReason?: string;
};

export type HandoverActionItem = {
	id: string;
	handoverId: string;
	description: string;
	isCompleted: boolean;
	createdAt: string;
	updatedAt: string;
	completedAt: string | null;
	priority?: Priority;
	dueTime?: string;
	createdBy?: string;
};

export type ActivityItem = {
	id: string;
	userId: string;
	userName: string;
	userInitials?: string;
	userColor?: string;
	action: string;
	section?: string;
	createdAt: string;
	type: "user_joined" | "content_updated" | "content_added" | "content_viewed";
};

export type ContingencyPlan = {
	id: string;
	handoverId: string;
	conditionText: string;
	actionText: string;
	priority: Priority;
	status: ContingencyStatus;
	createdBy: string;
	createdAt: string;
	updatedAt: string;
};

export type SituationAwareness = {
	handoverId: string;
	content: string | null;
	status: SituationAwarenessStatus;
	lastEditedBy: string;
	updatedAt: string;
};

export type Synthesis = {
	handoverId: string;
	content: string | null;
	status: string;
	lastEditedBy: string;
	updatedAt: string;
};

export type ClinicalData = {
	handoverId: string;
	illnessSeverity: IllnessSeverity;
	summaryText: string;
	lastEditedBy: string;
	status: string;
	updatedAt: string;
};

// =============================================================================
// Shift Check-In Types
// =============================================================================

export type ShiftCheckInPatient = {
	id: string | number;
	name: string;
	status: ShiftCheckInStatus;
	severity: IllnessSeverity;
	room: string;
	diagnosis: string;
	age?: number;
	assignedToName?: string | null;
};

export type Unit = {
	id: string;
	name: string;
};

export type Shift = {
	id: string;
	name: string;
	startTime?: string;
	endTime?: string;
};

// UI-specific config types (transformed from API types)
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

// =============================================================================
// User Types
// =============================================================================

export type User = {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	fullName: string;
	roles: Array<string>;
	isActive: boolean;
};

// =============================================================================
// UI-Only Types (not from API)
// =============================================================================

export type SyncStatus = "synced" | "syncing" | "pending" | "offline" | "error";

export type FullscreenComponent = "patient-summary" | "situation-awareness";

export type FullscreenEditingState = {
	component: FullscreenComponent;
	autoEdit: boolean;
};

export type ExpandedSections = {
	illness: boolean;
	patient: boolean;
	actions: boolean;
	awareness: boolean;
	synthesis: boolean;
};

export type Collaborator = {
	id: number;
	name: string;
	initials: string;
	color: string;
	status: "active" | "viewing" | "offline";
	lastSeen: string;
	activity: string;
	role: string;
	presenceType: "assigned-current" | "assigned-receiving" | "participating" | "supporting";
};

// =============================================================================
// Home/Dashboard UI Types
// =============================================================================

export type RecentPreview = {
	title: string;
	avatars: Array<{ src: string | null; fallback: string; bg: string }>;
	status: string;
	pr: string;
	color?: string;
};

export type SearchResult = {
	name: string;
	category: string;
	type: "handover" | "team" | "patient" | "assistant";
};

export type HandoverUI = {
	id: string;
	status: "Error" | "Ready";
	statusColor: string;
	environment: string;
	environmentColor: string;
	patientKey: string;
	patientName: string;
	patientIcon: {
		type: "text";
		value: string;
		bg: string;
		text?: string;
	};
	time: string;
	statusTime: string;
	environmentType: "Preview" | "Production";
	current?: boolean;
	bedLabel?: string;
	mrn?: string;
	author?: string;
	avatar?: string;
};

export type Metric = {
	label: string;
	value: string;
	tooltip: string;
	currentValue: string;
	totalValue: string;
};
