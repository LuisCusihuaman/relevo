import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useAssignPatients, useReadyHandover, usePendingHandovers } from "@/api";
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
	const assignMutation = useAssignPatients();
	const readyHandoverMutation = useReadyHandover();
	const { reset: resetPersistentState } = useShiftCheckInStore();
	const { data: pendingHandovers } = usePendingHandovers(userId);

	const submitCheckIn = useCallback(({ shiftId, patients, selectedIndexes }: SubmitCheckInParams) => {
		const selectedPatientIds = selectedIndexes
			.map((index) => patients[index]?.id)
			.filter((id): id is string => Boolean(id))
			.map((id) => String(id));

		const payload = { shiftId, patientIds: selectedPatientIds };
		
		assignMutation.mutate(payload, {
			onSuccess: () => {
				// After assignment, find the draft handovers for the selected patients and mark them as ready
				if (pendingHandovers?.handovers) {
					const draftHandovers = pendingHandovers.handovers.filter(h => 
						selectedPatientIds.includes(h.patientId) && h.stateName === "Draft"
					);
					draftHandovers.forEach(h => {
						readyHandoverMutation.mutate(h.id);
					});
				}

				window.localStorage.setItem("dailySetupCompleted", "true");
				resetPersistentState();
				void navigate({ to: "/" });
			},
			onError: (error) => {
				console.error('Assignment failed:', error);
			},
		});
	}, [assignMutation, navigate, resetPersistentState, pendingHandovers, readyHandoverMutation]);

	return {
		submitCheckIn,
		isAssigning: assignMutation.isPending,
	};
}
