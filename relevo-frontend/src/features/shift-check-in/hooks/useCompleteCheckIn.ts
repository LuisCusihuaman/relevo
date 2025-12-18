import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useAssignPatients, useReadyHandover, usePendingHandovers, handoverQueryKeys } from "@/api";
import { useShiftCheckInStore } from "@/store/shift-check-in.store";
import type { ShiftCheckInPatient } from "@/types/domain";

type SubmitCheckInParams = {
	shiftId: string;
	patients: Array<ShiftCheckInPatient>;
	selectedIndexes: Array<number>;
	userId: string;
};

export function useCompleteCheckIn(userId: string) {
	const navigate = useNavigate();
	const queryClient = useQueryClient();
	const assignMutation = useAssignPatients();
	const readyHandoverMutation = useReadyHandover();
	const { reset: resetPersistentState } = useShiftCheckInStore();
	const { refetch: refetchPendingHandovers } = usePendingHandovers(userId);

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
				
				// Retry logic: wait for handovers to be created (max 5 attempts, 500ms each)
				let attempts = 0;
				const maxAttempts = 5;
				let foundHandovers: Array<{ id: string; patientId: string; stateName: string }> = [];
				
				while (attempts < maxAttempts && foundHandovers.length < selectedPatientIds.length) {
					await new Promise(resolve => setTimeout(resolve, 500));
					
					// Refetch pending handovers to get the newly created ones
					const { data: updatedHandovers } = await refetchPendingHandovers();
					
					if (updatedHandovers?.handovers) {
						foundHandovers = updatedHandovers.handovers
							.filter(h => selectedPatientIds.includes(h.patientId))
							.map(h => ({ id: h.id, patientId: h.patientId, stateName: h.stateName }));
					}
					
					attempts++;
				}
				
				// Mark all draft handovers as ready
				const draftHandovers = foundHandovers.filter(h => h.stateName === "Draft");
				
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
	}, [assignMutation, navigate, resetPersistentState, readyHandoverMutation, queryClient, refetchPendingHandovers]);

	return {
		submitCheckIn,
		isAssigning: assignMutation.isPending,
	};
}
