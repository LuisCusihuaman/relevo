import { useActiveHandover, getSectionByType, getActionItems, type ActiveHandoverData, type PatientDetail } from "@/api";
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
	const { data: activeHandoverData, isLoading, error } = useActiveHandover();

	const patientData = useMemo((): PatientHandoverData | null => {
		if (!activeHandoverData?.handover) return null;

		const handover = activeHandoverData.handover;
		const patient = handover.patientName || "Unknown Patient";

		// Calculate approximate age (this would ideally come from patient details)
		const age = 14; // Default age, would be calculated from DOB in real implementation

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
			if (status === "inprogress" || status === "in_progress") {
				handoverStatus = "in-progress";
			} else if (status === "completed") {
				handoverStatus = "completed";
			}
		}

		// Get action items from sections
		const actionItems = getActionItems(activeHandoverData.sections);

		// Get assigned physician from participants (first active participant)
		const assignedPhysician = activeHandoverData.participants.find(p => p.status === "active") || {
			userName: "Dr. Current",
			userRole: "Attending Physician"
		};

		// Get initials from physician name (remove client-side logic)
		const getInitials = (name: string): string => {
			return name.split(' ').map(n => n[0]).join('').toUpperCase();
		};

		// Create physician objects with real data
		const assignedPhysicianData = {
			name: assignedPhysician.userName,
			role: assignedPhysician.userRole || "Attending Physician",
			initials: getInitials(assignedPhysician.userName),
			color: "bg-blue-600", // This would come from user preferences in real implementation
			shiftEnd: "17:00",
			status: "handing-off" as const,
			patientAssignment: "assigned" as const,
		};

		const receivingPhysicianData = {
			name: handover.assignedTo || "Dr. Next",
			role: "Evening Attending",
			initials: handover.assignedTo ? getInitials(handover.assignedTo) : "DN",
			color: "bg-purple-600",
			shiftStart: "17:00",
			status: "ready-to-receive" as const,
			patientAssignment: "receiving" as const,
		};

		return {
			id: handover.patientId,
			name: patient,
			age,
			mrn: "MRN-001", // This would come from patient details in real implementation
			admissionDate: handover.createdAt || new Date().toISOString(),
			currentDateTime: new Date().toLocaleString(),
			primaryTeam: handover.shiftName || "Day Shift",
			primaryDiagnosis: handover.patientSummary?.content || "Diagnosis pending",
			severity,
			handoverStatus,
			shift: `${handover.shiftName} → Next Shift`,
			room: "201", // This would come from patient details
			unit: "Internal Medicine", // This would come from patient details
			assignedPhysician: assignedPhysicianData,
			receivingPhysician: receivingPhysicianData,
			handoverTime: "17:00 PMT",
			actionItems,
		};
	}, [activeHandoverData]);

	return {
		patientData,
		isLoading,
		error,
	};
}
