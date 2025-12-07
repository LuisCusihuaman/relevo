import type { PatientHandoverData } from "@/api";

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
		| PatientHandoverData["assignedPhysician"]
		| PatientHandoverData["receivingPhysician"]
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
