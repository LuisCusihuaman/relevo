import type { ShiftCheckInPatient } from "@/types/domain";

function toStatus(s?: string): "pending" | "assigned" | "in-progress" | "complete" {
	return s === "pending" || s === "assigned" || s === "in-progress" || s === "complete" ? s : "pending";
}

function toSeverity(v?: string): "stable" | "watcher" | "unstable" {
	return v === "stable" || v === "watcher" || v === "unstable" ? v : "watcher";
}

export function transformApiPatient(apiPatient: {
	id: string | number;
	name: string;
	age?: number;
	room?: string | null;
	diagnosis?: string | null;
	status?: string;
	severity?: string;
}): ShiftCheckInPatient {
	return {
		id: apiPatient.id,
		name: apiPatient.name,
		age: apiPatient.age,
		room: apiPatient.room ?? "",
		diagnosis: apiPatient.diagnosis ?? "",
		status: toStatus(apiPatient.status),
		severity: toSeverity(apiPatient.severity),
	};
}
