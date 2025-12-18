/**
 * Hook to get upcoming action items from assigned patients
 * Rule: Concise-FP - Functional, no classes
 */
import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useAssignedPatients } from "@/api";
import { getHandoverActionItems } from "@/api/endpoints/handovers";
import type { HandoverActionItem, PatientSummaryCard } from "@/types/domain";

type UpcomingActionItem = HandoverActionItem & {
	patientId: string;
	patientName: string;
};

/**
 * Parse dueTime string to Date
 * Handles formats like "2PM", "14:00", "2:00 PM", "en 2 horas", etc.
 * Rule: Concise-FP - Functional, no classes
 */
function parseDueTime(dueTime: string | undefined): Date | null {
	if (!dueTime) return null;

	const now = new Date();
	const lowerDueTime = dueTime.toLowerCase().trim();

	// Try to parse relative times first: "en 2 horas", "dentro de 3 horas", "en 1 hora"
	const relativeMatch = lowerDueTime.match(/(?:en|dentro de|in|within)\s*(\d+)\s*(?:hora|hour)/i);
	if (relativeMatch) {
		const hours = parseInt(relativeMatch[1] ?? "0", 10);
		if (hours > 0 && hours <= 3) {
			const dueDate = new Date(now);
			dueDate.setHours(dueDate.getHours() + hours);
			return dueDate;
		}
	}

	// Try to parse time formats: "2PM", "14:00", "2:00 PM", "14:30", etc.
	const timeMatch = lowerDueTime.match(/(\d{1,2}):?(\d{2})?\s*(am|pm)?/i);
	if (timeMatch) {
		let hours = parseInt(timeMatch[1] ?? "0", 10);
		const minutes = parseInt(timeMatch[2] ?? "0", 10);
		const period = timeMatch[3]?.toLowerCase();

		if (period === "pm" && hours !== 12) {
			hours += 12;
		} else if (period === "am" && hours === 12) {
			hours = 0;
		}

		if (hours >= 0 && hours < 24) {
			const dueDate = new Date(now);
			dueDate.setHours(hours, minutes, 0, 0);

			// If the time has passed today, assume it's for tomorrow
			if (dueDate < now) {
				dueDate.setDate(dueDate.getDate() + 1);
			}

			// Only return if it's within the next 3 hours
			const threeHoursFromNow = new Date(now.getTime() + 3 * 60 * 60 * 1000);
			if (dueDate <= threeHoursFromNow) {
				return dueDate;
			}
		}
	}

	// Default: if we can't parse, return null
	return null;
}

/**
 * Check if action is due within the next 3 hours
 */
function isDueWithin3Hours(dueTime: string | undefined): boolean {
	const dueDate = parseDueTime(dueTime);
	if (!dueDate) return false;

	const now = new Date();
	const threeHoursFromNow = new Date(now.getTime() + 3 * 60 * 60 * 1000);

	return dueDate >= now && dueDate <= threeHoursFromNow;
}

export function useUpcomingActions(): {
	upcomingActions: Array<UpcomingActionItem>;
	isLoading: boolean;
	error: Error | null;
} {
	const { data: assignedPatientsData, isLoading: isLoadingPatients } = useAssignedPatients();

	// Get all handover IDs from assigned patients
	const handoverIds = useMemo(() => {
		if (!assignedPatientsData?.items) return [];
		return assignedPatientsData.items
			.filter((p: PatientSummaryCard) => p.handoverId !== null)
			.map((p: PatientSummaryCard) => p.handoverId as string);
	}, [assignedPatientsData]);

	// Fetch action items for all handovers in parallel
	const actionItemsQueries = useQuery({
		queryKey: ["upcomingActions", handoverIds],
		queryFn: async () => {
			if (handoverIds.length === 0) return [];

			const promises = handoverIds.map(async (handoverId) => {
				try {
					const response = await getHandoverActionItems(handoverId);
					return response.actionItems.map((item) => ({
						...item,
						handoverId,
					}));
				} catch (error) {
					console.error(`Error fetching actions for handover ${handoverId}:`, error);
					return [];
				}
			});

			const results = await Promise.all(promises);
			return results.flat();
		},
		enabled: handoverIds.length > 0 && !isLoadingPatients,
		staleTime: 1000 * 60 * 2, // 2 minutes
		gcTime: 1000 * 60 * 5, // 5 minutes
	});

	// Filter and enrich with patient information
	const upcomingActions = useMemo(() => {
		if (!assignedPatientsData?.items || !actionItemsQueries.data) return [];

		const allActions: Array<UpcomingActionItem> = [];

		// Create a map of handoverId -> patient
		const handoverToPatient = new Map<string, PatientSummaryCard>();
		assignedPatientsData.items.forEach((patient) => {
			if (patient.handoverId) {
				handoverToPatient.set(patient.handoverId, patient);
			}
		});

		// Filter actions that are due within 3 hours and not completed
		actionItemsQueries.data.forEach((action) => {
			if (!action.isCompleted && isDueWithin3Hours(action.dueTime)) {
				const patient = handoverToPatient.get(action.handoverId);
				if (patient) {
					allActions.push({
						...action,
						patientId: patient.id,
						patientName: patient.name,
					});
				}
			}
		});

		// Sort by dueTime (earliest first)
		return allActions.sort((a, b) => {
			const dateA = parseDueTime(a.dueTime);
			const dateB = parseDueTime(b.dueTime);
			if (!dateA) return 1;
			if (!dateB) return -1;
			return dateA.getTime() - dateB.getTime();
		});
	}, [assignedPatientsData, actionItemsQueries.data]);

	return {
		upcomingActions,
		isLoading: isLoadingPatients || actionItemsQueries.isLoading,
		error: actionItemsQueries.error,
	};
}

