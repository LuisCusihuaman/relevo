import { useActiveHandover, getActionItems } from "@/api";
import { usePatientDetails } from "@/api/endpoints/patients";
import { useMemo } from "react";

export interface PatientHandoverData {
	id: string;
	name: string;
	age: number;
	mrn: string;
	admissionDate: string;
	currentDateTime: string;
	primaryTeam: string;
	primaryDiagnosis: string;
	severity: "stable" | "watcher" | "unstable";
	handoverStatus: "not-started" | "in-progress" | "completed";
	shift: string;
	room: string;
	unit: string;
	assignedPhysician: {
		name: string;
		role: string;
		initials: string;
		color: string;
		shiftEnd: string;
		status: "handing-off" | "receiving";
		patientAssignment: "assigned" | "receiving";
	};
	receivingPhysician: {
		name: string;
		role: string;
		initials: string;
		color: string;
		shiftStart: string;
		status: "ready-to-receive" | "receiving";
		patientAssignment: "receiving";
	};
	handoverTime: string;
	actionItems: Array<{ id: string; description: string; isCompleted: boolean }>;
}

export function usePatientHandoverData(): {
	patientData: PatientHandoverData | null;
	isLoading: boolean;
	error: Error | null;
} {
	// Get active handover data from backend
	const { data: activeHandoverData, isLoading: isHandoverLoading, error: handoverError } = useActiveHandover();

	// Get patient details if we have handover data with patient ID
	const patientId = activeHandoverData?.handover?.patientId;
	const { data: patientDetails, isLoading: isPatientLoading, error: patientError } = usePatientDetails(patientId || "");

	const isLoading = isHandoverLoading || (patientId ? isPatientLoading : false);
	const error = handoverError || patientError;

	const patientData = useMemo((): PatientHandoverData | null => {
		if (!activeHandoverData?.handover) return null;

		const handover = activeHandoverData.handover;

		// Use patient details from API if available, otherwise fallback to handover data
		const patientName = patientDetails?.name || handover.patientName || "Unknown Patient";

		// Calculate age from patient details or estimate from handover data
		let age = 0;
		if (patientDetails?.dob) {
			const birthDate = new Date(patientDetails.dob);
			const today = new Date();
			age = today.getFullYear() - birthDate.getFullYear();
			const monthDiff = today.getMonth() - birthDate.getMonth();
			if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
				age--;
			}
		}

		// Get severity from handover data
		let severity: "stable" | "watcher" | "unstable" = "stable";
		if (handover.illnessSeverity?.severity) {
			const handoverSeverity = handover.illnessSeverity.severity.toLowerCase();
			if (handoverSeverity === "watcher" || handoverSeverity === "unstable") {
				severity = handoverSeverity as "watcher" | "unstable";
			}
		}

		// Map handover status
		let handoverStatus: "not-started" | "in-progress" | "completed" = "not-started";
		if (handover.status) {
			const status = handover.status.toLowerCase();
			if (status === "inprogress" || status === "in_progress" || status === "active") {
				handoverStatus = "in-progress";
			} else if (status === "completed") {
				handoverStatus = "completed";
			}
		}

		// Get action items from sections
		const actionItems = getActionItems(activeHandoverData.sections);

		// Get initials from physician name helper
		const getInitials = (name: string): string => {
			return name.split(' ').map(n => n[0]).join('').toUpperCase();
		};

		// Find physicians from participants
		const assignedParticipant = activeHandoverData.participants.find(p =>
			p.userRole?.toLowerCase().includes("day") ||
			p.userRole?.toLowerCase().includes("attending") ||
			p.status === "active"
		);

		const receivingParticipant = activeHandoverData.participants.find(p =>
			p.userRole?.toLowerCase().includes("evening") ||
			p.userRole?.toLowerCase().includes("night") ||
			p.userRole?.toLowerCase().includes("next")
		);

		// Create physician objects with real data
		const assignedPhysicianData = {
			name: assignedParticipant?.userName || handover.createdBy || "Dr. Current",
			role: assignedParticipant?.userRole || "Attending Physician",
			initials: getInitials(assignedParticipant?.userName || handover.createdBy || "Dr. Current"),
			color: "bg-blue-600",
			shiftEnd: "17:00",
			status: "handing-off" as const,
			patientAssignment: "assigned" as const,
		};

		const receivingPhysicianData = {
			name: receivingParticipant?.userName || handover.assignedTo || "Dr. Next",
			role: receivingParticipant?.userRole || "Evening Attending",
			initials: getInitials(receivingParticipant?.userName || handover.assignedTo || "Dr. Next"),
			color: "bg-purple-600",
			shiftStart: "17:00",
			status: "ready-to-receive" as const,
			patientAssignment: "receiving" as const,
		};

		return {
			id: handover.patientId,
			name: patientName,
			age,
			mrn: patientDetails?.mrn || `MRN-${handover.patientId?.split('-').pop()?.padStart(3, '0') || '001'}`,
			admissionDate: patientDetails?.admissionDate || handover.createdAt || new Date().toISOString(),
			currentDateTime: new Date().toLocaleString(),
			primaryTeam: patientDetails?.currentUnit || handover.shiftName || "Medical Team",
			primaryDiagnosis: patientDetails?.diagnosis || handover.patientSummary?.content || "Diagnosis pending",
			severity,
			handoverStatus,
			shift: `${handover.shiftName || "Current"} → ${receivingParticipant?.userRole || "Next Shift"}`,
			room: patientDetails?.roomNumber || "201",
			unit: patientDetails?.currentUnit || "Internal Medicine",
			assignedPhysician: assignedPhysicianData,
			receivingPhysician: receivingPhysicianData,
			handoverTime: "17:00",
			actionItems,
		};
	}, [activeHandoverData, patientDetails]);

	return {
		patientData,
		isLoading,
		error,
	};
}
