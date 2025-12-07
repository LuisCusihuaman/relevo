export type IllnessSeverity = "stable" | "watcher" | "unstable";

export type HandoverStatus =
	| "Draft"
	| "Ready"
	| "InProgress"
	| "Accepted"
	| "Completed"
	| "Cancelled"
	| "Rejected"
	| "Expired";

export type ShiftCheckInStatus = "pending" | "in-progress" | "complete";

export type SituationAwarenessStatus =
	| "Draft"
	| "Ready"
	| "InProgress"
	| "Completed";

// Use (string & {}) to preserve autocompletion for known values while allowing any string
export type UserRole = "physician" | "nurse" | "admin" | "student" | (string & {});

export interface UnitConfig {
	id: string;
	name: string;
	description: string;
}

export interface ShiftConfig {
	id: string;
	name: string;
	time: string;
}

export interface Patient {
	id: string;
	name: string;
	mrn: string;
	dob?: string;
	age?: number;
	gender?: "Male" | "Female" | "Other" | "Unknown";
	admissionDate?: string;
	room?: string;
	bed?: string;
	unit?: string;
	diagnosis?: string;
	illnessSeverity?: IllnessSeverity;
	primaryTeam?: string;
	allergies?: Array<string>;
	medications?: Array<string>;
	notes?: string;
}

export interface ShiftCheckInPatient extends Omit<Partial<Patient>, "id"> {
	id: string | number;
	name: string;
	status: ShiftCheckInStatus;
	severity: IllnessSeverity;
	room: string;
	diagnosis: string;
}

export interface ActionItem {
	id: string;
	handoverId?: string;
	description: string;
	priority: "low" | "medium" | "high";
	dueTime?: string;
	isCompleted: boolean;
	createdAt: string;
	completedAt?: string | null;
	createdBy?: string;
}

export interface ActivityItem {
	id: string;
	userId: string;
	userName: string;
	userInitials?: string;
	userColor?: string;
	action: string; // "activityType" in API
	section?: string;
	createdAt: string;
	type:
		| "user_joined"
		| "content_updated"
		| "content_added"
		| "content_viewed";
}

export interface ContingencyPlan {
	id: string;
	handoverId?: string;
	condition: string;
	action: string;
	priority: "low" | "medium" | "high";
	status: "active" | "planned" | "completed";
	createdAt: string;
	createdBy: string;
}

// UI & Store Types

export type SyncStatus =
	| "synced"
	| "syncing"
	| "pending"
	| "offline"
	| "error";

export type FullscreenComponent = "patient-summary" | "situation-awareness";

export interface FullscreenEditingState {
	component: FullscreenComponent;
	autoEdit: boolean;
}

export interface ExpandedSections {
	illness: boolean;
	patient: boolean;
	actions: boolean;
	awareness: boolean;
	synthesis: boolean;
}

export interface Collaborator {
	id: number;
	name: string;
	initials: string;
	color: string;
	status: "active" | "viewing" | "offline";
	lastSeen: string;
	activity: string;
	role: string;
	presenceType:
		| "assigned-current"
		| "assigned-receiving"
		| "participating"
		| "supporting";
}
