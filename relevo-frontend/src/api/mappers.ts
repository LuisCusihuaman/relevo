import type { Handover as ApiHandover, PatientSummaryCard } from "./types";
import type { Handover as UiHandover } from "@/components/home/types";

// Configuration for mapping API states to UI properties
type UiStateConfig = {
	status: "Error" | "Ready";
	statusColor: string;
	environment: string;
	environmentColor: string;
	isActive: boolean;
};

const DEFAULT_STATE_CONFIG: UiStateConfig = {
	status: "Error",
	statusColor: "bg-red-500",
	environment: "Unknown",
	environmentColor: "text-gray-600",
	isActive: false,
};

const HANDOVER_STATE_MAP: Record<string, UiStateConfig> = {
	Draft: {
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Draft",
		environmentColor: "text-red-600",
		isActive: true,
	},
	Ready: {
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Ready",
		environmentColor: "text-red-600",
		isActive: true,
	},
	InProgress: {
		status: "Error",
		statusColor: "bg-red-500",
		environment: "In Progress",
		environmentColor: "text-red-600",
		isActive: true,
	},
	Accepted: {
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Accepted",
		environmentColor: "text-red-600",
		isActive: true,
	},
	Completed: {
		status: "Ready",
		statusColor: "bg-green-500",
		environment: "Completed",
		environmentColor: "text-green-600",
		isActive: false,
	},
	Cancelled: {
		status: "Ready",
		statusColor: "bg-green-500",
		environment: "Cancelled",
		environmentColor: "text-green-600",
		isActive: false,
	},
	Rejected: {
		status: "Ready",
		statusColor: "bg-green-500",
		environment: "Rejected",
		environmentColor: "text-green-600",
		isActive: false,
	},
	Expired: {
		status: "Ready",
		statusColor: "bg-green-500",
		environment: "Expired",
		environmentColor: "text-green-600",
		isActive: false,
	},
	// PatientSummaryCard statuses
	NotStarted: {
		status: "Error",
		statusColor: "bg-gray-500",
		environment: "Not Started",
		environmentColor: "text-gray-600",
		isActive: false,
	},
	Active: {
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Active",
		environmentColor: "text-red-600",
		isActive: true,
	},
};

function getUiConfig(stateName: string): UiStateConfig {
	return HANDOVER_STATE_MAP[stateName] || DEFAULT_STATE_CONFIG;
}

// Get first letter of patient name
const getInitials = (name: string): string => {
	return name.charAt(0).toUpperCase();
};

// Generate patient key from patient ID
const getPatientKey = (id: string): string => {
	return id.toLowerCase().replace(/[^a-z0-9]/g, "-");
};

/**
 * Mapping function to convert API Handover to UI Handover type
 */
export function mapApiHandoverToUiHandover(
	apiHandover: ApiHandover,
): UiHandover {
	const config = getUiConfig(apiHandover.stateName);

	// Use patient name from handover if available, otherwise fallback to patient ID
	// Since patientName is removed from API, we use a placeholder or fetch it separately.
	// For now, we use "Patient {ID}" as placeholder.
	const patientName = `Patient ${apiHandover.patientId}`;

	return {
		id: apiHandover.id,
		status: config.status,
		statusColor: config.statusColor,
		environment: config.environment,
		environmentColor: config.environmentColor,
		patientKey: getPatientKey(apiHandover.patientId),
		patientName: patientName,
		patientIcon: {
			type: "text",
			value: getInitials(patientName),
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: apiHandover.createdAt || new Date().toISOString(), // Use created date from API or current date as fallback
		statusTime: config.isActive ? "Active" : "Inactive",
		environmentType: "Preview", // Default to Preview
		current: config.isActive,
		author: apiHandover.createdBy || "System",
		avatar: "", // Default empty avatar
	};
}

/**
 * Mapping function to convert API PatientSummaryCard to UI Handover type
 */
export function mapApiPatientToUiHandover(
	apiPatient: PatientSummaryCard,
): UiHandover {
	const config = getUiConfig(apiPatient.handoverStatus);

	return {
		id: apiPatient.handoverId || apiPatient.id,
		status: config.status,
		statusColor: config.statusColor,
		environment: config.environment,
		environmentColor: config.environmentColor,
		patientKey: getPatientKey(apiPatient.id),
		patientName: apiPatient.name,
		patientIcon: {
			type: "text",
			value: getInitials(apiPatient.name),
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: new Date().toISOString(), // Use current date as fallback
		statusTime: "Active",
		environmentType: "Preview",
		current: config.isActive,
		author: "System",
		avatar: "",
	};
}
