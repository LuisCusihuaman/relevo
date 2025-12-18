import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useAssignPatients, useReadyHandover, handoverQueryKeys } from "@/api";
import { getPatientHandoverTimeline } from "@/api/endpoints/patients";
import { useShiftCheckInStore } from "@/store/shift-check-in.store";
import type { ShiftCheckInPatient } from "@/types/domain";

type SubmitCheckInParams = {
	shiftId: string;
	patients: Array<ShiftCheckInPatient>;
	selectedIndexes: Array<number>;
	userId: string;
};

export function useCompleteCheckIn(_userId: string) {
	const navigate = useNavigate();
	const queryClient = useQueryClient();
	const assignMutation = useAssignPatients();
	const readyHandoverMutation = useReadyHandover();
	const { reset: resetPersistentState } = useShiftCheckInStore();

	const submitCheckIn = useCallback(({ shiftId, patients, selectedIndexes }: SubmitCheckInParams) => {
		const selectedPatientIds = selectedIndexes
			.map((index) => patients[index]?.id)
			.filter((id): id is string => Boolean(id))
			.map((id) => String(id));

		const payload = { shiftId, patientIds: selectedPatientIds };
		
		assignMutation.mutate(payload, {
			onSuccess: async () => {
				// Handovers are created asynchronously, so we need to wait and retry
				// Invalidate handover queries to trigger a refetch
				await queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
				
				// Retry logic: wait for handovers to be created (max 8 attempts, 750ms each = 6 seconds total)
				let attempts = 0;
				const maxAttempts = 8;
				const foundHandoversMap = new Map<string, { id: string; patientId: string; stateName: string }>();
				
				while (attempts < maxAttempts && foundHandoversMap.size < selectedPatientIds.length) {
					await new Promise(resolve => setTimeout(resolve, 750));
					
					// For each patient, check if handover was created by querying their timeline
					await Promise.all(
						selectedPatientIds.map(async (patientId) => {
							if (foundHandoversMap.has(patientId)) return; // Already found
							
							try {
								const timeline = await queryClient.fetchQuery({
									queryKey: ["patients", "handoverTimeline", patientId, { page: 1, pageSize: 5 }],
									queryFn: () => getPatientHandoverTimeline(patientId, { page: 1, pageSize: 5 }),
									staleTime: 0, // Always fetch fresh
								});
								
								// Find the most recent active handover (Draft, Ready, or InProgress)
								const activeHandover = timeline?.items?.find(h => 
									h.stateName === "Draft" || h.stateName === "Ready" || h.stateName === "InProgress"
								);
								
								if (activeHandover) {
									foundHandoversMap.set(patientId, {
										id: activeHandover.id,
										patientId: activeHandover.patientId,
										stateName: activeHandover.stateName,
									});
								}
							} catch (error) {
								console.warn(`Failed to fetch handover for patient ${patientId}:`, error);
							}
						})
					);
					
					attempts++;
				}
				
				// Mark all draft handovers as ready
				const draftHandovers = Array.from(foundHandoversMap.values()).filter(h => h.stateName === "Draft");
				
				if (draftHandovers.length > 0) {
					await Promise.all(
						draftHandovers.map(h => 
							new Promise<void>((resolve) => {
								readyHandoverMutation.mutate(h.id, {
									onSuccess: () => {
										// Invalidate queries after marking as ready
										void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
										resolve();
									},
									onError: () => resolve(), // Continue even if one fails
								});
							})
						)
					);
				}

				window.localStorage.setItem("dailySetupCompleted", "true");
				resetPersistentState();
				void navigate({ to: "/" });
			},
			onError: (error) => {
				console.error('Assignment failed:', error);
			},
		});
	}, [assignMutation, navigate, resetPersistentState, readyHandoverMutation, queryClient]);

	return {
		submitCheckIn,
		isAssigning: assignMutation.isPending,
	};
}
