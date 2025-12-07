import type { PhysicianAssignment } from "@/api";

export function formatDiagnosis(input: unknown): string {
	if (typeof input === "string") return input;
	if (
		input &&
		typeof input === "object" &&
		"primary" in input &&
		typeof (input as { primary: unknown }).primary === "string"
	) {
		const diagnosis = input as { primary: string; secondary?: Array<string> };
		const secondary = Array.isArray(diagnosis.secondary)
			? ` — ${diagnosis.secondary.join(", ")}`
			: "";
		return `${diagnosis.primary}${secondary}`;
	}
	return "";
}

export const getInitials = (name?: string | null): string => {
	if (!name) return "U";
	return name
		.split(" ")
		.map((n) => n[0])
		.join("")
		.toUpperCase();
};

export const formatPhysician = (
	physician:
		| PhysicianAssignment
		| null
		| undefined,
): { name: string; initials: string; role: string } => {
	if (!physician) {
		return {
			name: "Unknown",
			initials: "U",
			role: "Doctor",
		};
	}
	return {
		name: physician.name,
		initials: getInitials(physician.name),
		role: physician.role || "Doctor",
	};
};

const monthMap: Record<string, string> = {
	Jan: "Ene",
	Feb: "Feb",
	Mar: "Mar",
	Apr: "Abr",
	May: "May",
	Jun: "Jun",
	Jul: "Jul",
	Aug: "Ago",
	Sep: "Sep",
	Oct: "Oct",
	Nov: "Nov",
	Dec: "Dic",
};

export const formatRelativeTime = (value: string): string => {
	let s = value;
	s = s.replace(/(\d+)\s*d\s*ago/g, "hace $1 d");
	s = s.replace(/ago/g, "");
	s = s.replace(/\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b/g, (m) => monthMap[m] || m);
	return s.trim();
};

export const getSeverityColor = (severity: string): string => {
	switch (severity.toLowerCase()) {
		case "unstable":
			return "text-red-600";
		case "watcher":
			return "text-yellow-600";
		case "stable":
			return "text-green-600";
		default:
			return "text-gray-600";
	}
};

export const getSeverityBadgeColor = (severity: string): string => {
	switch (severity.toLowerCase()) {
		case "unstable":
		case "critical":
			return "bg-red-100 text-red-800 border-red-200";
		case "watcher":
		case "guarded":
			return "bg-yellow-100 text-yellow-800 border-yellow-200";
		case "stable":
			return "bg-green-100 text-green-800 border-green-200";
		default:
			return "bg-gray-100 text-gray-800 border-gray-200";
	}
};

export const getStatusColor = (status: string): string => {
	switch (status.toLowerCase()) {
		case "pending":
			return "text-orange-600";
		case "in-progress":
		case "inprogress":
			return "text-blue-600";
		case "complete":
		case "completed":
			return "text-green-600";
		default:
			return "text-gray-600";
	}
};

export const getStatusBadgeColor = (status: string): string => {
	switch (status.toLowerCase()) {
		case "completed":
		case "complete":
			return "bg-green-100 text-green-800";
		case "in-progress":
		case "inprogress":
			return "bg-blue-100 text-blue-800";
		default:
			return "bg-gray-100 text-gray-800";
	}
};
