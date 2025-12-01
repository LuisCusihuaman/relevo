import type { Handover as ApiHandover, PatientSummaryCard } from "./types";
import type { Handover as UiHandover } from "@/components/home/types";

/**
 * Mapping function to convert API Handover to UI Handover type
 */
export function mapApiHandoverToUiHandover(apiHandover: ApiHandover): UiHandover {
	// Generate patient key from patient ID
	const patientKey = apiHandover.patientId.toLowerCase().replace(/[^a-z0-9]/g, "-");

	// Map handover status to UI status
	const getStatusFromHandoverStatus = (stateName: ApiHandover["stateName"]): "Error" | "Ready" => {
		switch (stateName) {
			case "Draft":
			case "Ready":
			case "InProgress":
			case "Accepted":
				return "Error"; // In the UI, "Error" seems to mean "In Progress" / Active
			case "Completed":
			case "Cancelled":
			case "Rejected":
			case "Expired":
				return "Ready";
			default:
				return "Error";
		}
	};

	// Map status colors
	const getStatusColor = (stateName: ApiHandover["stateName"]): string => {
		switch (stateName) {
			case "Draft":
			case "Ready":
			case "InProgress":
			case "Accepted":
				return "bg-red-500";
			case "Completed":
			case "Cancelled":
			case "Rejected":
			case "Expired":
				return "bg-green-500";
			default:
				return "bg-red-500";
		}
	};

	// Map environment
	const getEnvironment = (stateName: ApiHandover["stateName"]): string => {
		switch (stateName) {
			case "Draft":
				return "Draft";
			case "Ready":
				return "Ready";
			case "InProgress":
				return "In Progress";
			case "Accepted":
				return "Accepted";
			case "Completed":
				return "Completed";
			case "Cancelled":
				return "Cancelled";
			case "Rejected":
				return "Rejected";
			case "Expired":
				return "Expired";
			default:
				return "Unknown";
		}
	};

	// Map environment color
	const getEnvironmentColor = (stateName: ApiHandover["stateName"]): string => {
		switch (stateName) {
			case "Draft":
			case "Ready":
			case "InProgress":
			case "Accepted":
				return "text-red-600";
			case "Completed":
			case "Cancelled":
			case "Rejected":
			case "Expired":
				return "text-green-600";
			default:
				return "text-gray-600";
		}
	};

	// Get first letter of patient name
	const getInitials = (name: string): string => {
		return name.charAt(0).toUpperCase();
	};

	// Use patient name from handover if available, otherwise fallback to patient ID
	// Since patientName is removed from API, we use a placeholder or fetch it separately.
	// For now, we use "Patient {ID}" as placeholder.
	const patientName = `Patient ${apiHandover.patientId}`;

	return {
		id: apiHandover.id,
		status: getStatusFromHandoverStatus(apiHandover.stateName),
		statusColor: getStatusColor(apiHandover.stateName),
		environment: getEnvironment(apiHandover.stateName),
		environmentColor: getEnvironmentColor(apiHandover.stateName),
		patientKey: patientKey,
		patientName: patientName,
		patientIcon: {
			type: "text",
			value: getInitials(patientName),
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: apiHandover.createdAt || new Date().toISOString(), // Use created date from API or current date as fallback
		statusTime: ["Draft", "Ready", "InProgress", "Accepted"].includes(apiHandover.stateName) ? "Active" : "Inactive",
		environmentType: "Preview", // Default to Preview
		current: ["Draft", "Ready", "InProgress", "Accepted"].includes(apiHandover.stateName),
		author: apiHandover.createdBy || "System",
		avatar: "", // Default empty avatar
	};
}

/**
 * Mapping function to convert API PatientSummaryCard to UI Handover type
 */
export function mapApiPatientToUiHandover(apiPatient: PatientSummaryCard): UiHandover {
	// Generate patient key from patient ID
	const patientKey = apiPatient.id.toLowerCase().replace(/[^a-z0-9]/g, "-");

	// Map handover status to UI status
	const getStatusFromHandoverStatus = (status: PatientSummaryCard["handoverStatus"]): "Error" | "Ready" => {
		switch (status) {
			case "NotStarted":
				return "Error"; // Not started means "Error" in UI terms
			case "InProgress":
			case "Active":
				return "Error"; // In progress means "Error" in UI terms
			case "Completed":
				return "Ready";
			default:
				return "Error";
		}
	};

	// Map status colors
	const getStatusColor = (status: PatientSummaryCard["handoverStatus"]): string => {
		switch (status) {
			case "NotStarted":
				return "bg-gray-500";
			case "Active":
			case "InProgress":
				return "bg-red-500";
			case "Completed":
				return "bg-green-500";
			default:
				return "bg-gray-500";
		}
	};

	// Map environment
	const getEnvironment = (status: PatientSummaryCard["handoverStatus"]): string => {
		switch (status) {
			case "NotStarted":
				return "Not Started";
			case "Active":
				return "Active";
			case "InProgress":
				return "In Progress";
			case "Completed":
				return "Completed";
			default:
				return "Unknown";
		}
	};

	// Map environment color
	const getEnvironmentColor = (status: PatientSummaryCard["handoverStatus"]): string => {
		switch (status) {
			case "NotStarted":
				return "text-gray-600";
			case "Active":
			case "InProgress":
				return "text-red-600";
			case "Completed":
				return "text-green-600";
			default:
				return "text-gray-600";
		}
	};

	// Get first letter of patient name
	const getInitials = (name: string): string => {
		return name.charAt(0).toUpperCase();
	};

	return {
		id: apiPatient.handoverId || apiPatient.id,
		status: getStatusFromHandoverStatus(apiPatient.handoverStatus),
		statusColor: getStatusColor(apiPatient.handoverStatus),
		environment: getEnvironment(apiPatient.handoverStatus),
		environmentColor: getEnvironmentColor(apiPatient.handoverStatus),
		patientKey: patientKey,
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
		current: apiPatient.handoverStatus === "Active" || apiPatient.handoverStatus === "InProgress",
		author: "System",
		avatar: "",
	};
}
