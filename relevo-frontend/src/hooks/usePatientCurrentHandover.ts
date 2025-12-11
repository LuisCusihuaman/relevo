import { useMemo } from "react";
import { usePatientHandoverTimeline } from "@/api/endpoints/patients";
import type { HandoverSummary } from "@/types/domain";

/**
 * Hook to resolve the current active handover for a patient.
 * 
 * Rule: Strict-TS - Explicit return types
 * 
 * An "active" handover is one with stateName in ['Draft', 'Ready', 'InProgress'].
 * Completed and Cancelled handovers are considered historical.
 * 
 * @param patientId - The patient ID to find the active handover for
 */

type PatientCurrentHandoverResult = {
	currentHandover: HandoverSummary | null;
	hasActiveHandover: boolean;
	isLoading: boolean;
	error: Error | null;
	timeline: Array<HandoverSummary>;
};

const ACTIVE_STATES = ["Draft", "Ready", "InProgress"] as const;

export function usePatientCurrentHandover(patientId: string): PatientCurrentHandoverResult {
	const { 
		data: timeline, 
		isLoading, 
		error 
	} = usePatientHandoverTimeline(patientId, { pageSize: 25 });

	const currentHandover = useMemo((): HandoverSummary | null => {
		if (!timeline?.items) return null;

		// Find first handover with active state
		return timeline.items.find((h) =>
			ACTIVE_STATES.includes(h.stateName as typeof ACTIVE_STATES[number])
		) ?? null;
	}, [timeline]);

	return useMemo((): PatientCurrentHandoverResult => ({
		currentHandover,
		hasActiveHandover: currentHandover !== null,
		isLoading,
		error: error as Error | null,
		timeline: timeline?.items ?? [],
	}), [currentHandover, isLoading, error, timeline?.items]);
}
