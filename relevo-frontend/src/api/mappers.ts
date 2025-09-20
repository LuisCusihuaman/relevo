import type { Handover as ApiHandover, PatientSummaryCard } from "./types";
import type { Handover as UiHandover } from "@/components/home/types";

/**
 * Mapping function to convert API Handover to UI Handover type
 */
export function mapApiHandoverToUiHandover(apiHandover: ApiHandover): UiHandover {
	// Generate patient key from patient ID
	const patientKey = apiHandover.patientId.toLowerCase().replace(/[^a-z0-9]/g, "-");

	// Map handover status to UI status
	const getStatusFromHandoverStatus = (status: ApiHandover["status"]): "Error" | "Ready" => {
		switch (status) {
			case "InProgress":
				return "Error"; // In the UI, "Error" seems to mean "In Progress"
			case "Completed":
				return "Ready";
			default:
				return "Error";
		}
	};

	// Map status colors
	const getStatusColor = (status: ApiHandover["status"]): string => {
		switch (status) {
			case "InProgress":
				return "bg-red-500";
			case "Completed":
				return "bg-green-500";
			default:
				return "bg-red-500";
		}
	};

	// Map environment
	const getEnvironment = (status: ApiHandover["status"]): string => {
		switch (status) {
			case "InProgress":
				return "In Progress";
			case "Completed":
				return "Completed";
			default:
				return "Unknown";
		}
	};

	// Map environment color
	const getEnvironmentColor = (status: ApiHandover["status"]): string => {
		switch (status) {
			case "InProgress":
				return "text-red-600";
			case "Completed":
				return "text-green-600";
			default:
				return "text-gray-600";
		}
	};

	// Get first letter of patient name (we don't have patient name in handover, so use patient ID)
	const getInitials = (patientId: string): string => {
		return patientId.charAt(0).toUpperCase();
	};

	return {
		id: apiHandover.id,
		status: getStatusFromHandoverStatus(apiHandover.status),
		statusColor: getStatusColor(apiHandover.status),
		environment: getEnvironment(apiHandover.status),
		environmentColor: getEnvironmentColor(apiHandover.status),
		patientKey: patientKey,
		patientName: `Patient ${apiHandover.patientId}`, // We don't have patient name in handover
		patientIcon: {
			type: "text",
			value: getInitials(apiHandover.patientId),
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: "Recently", // We'll need to calculate this based on creation date
		statusTime: "Active",
		environmentType: "Preview", // Default to Preview
		current: apiHandover.status === "InProgress",
		author: "System", // Default author since API doesn't provide this
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
		time: "Recently",
		statusTime: "Active",
		environmentType: "Preview",
		current: apiPatient.handoverStatus === "InProgress",
		author: "System",
		avatar: "",
	};
}
