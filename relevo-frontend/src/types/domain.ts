export type SeverityLevel = "stable" | "watcher" | "unstable";

export type ShiftCheckInStatus = "pending" | "in-progress" | "complete";

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

export interface ShiftCheckInPatient {
	id: string | number;
	name: string;
	age?: number;
	room: string;
	diagnosis: string;
	status: ShiftCheckInStatus;
	severity: SeverityLevel;
}

// UI & Store Types

export type SyncStatus = "synced" | "syncing" | "pending" | "offline" | "error";

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
