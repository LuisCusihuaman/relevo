import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../client";
import type {
	PaginatedHandovers,
	Handover,
	HandoverMessage,
	HandoverActivityItem,
	HandoverChecklistItem,
	HandoverContingencyPlan,
	SituationAwarenessResponse,
	ContingencyPlansResponse,
	UpdateSituationAwarenessRequest,
	ApiResponse,
	SynthesisResponse,
	UpdatePatientDataRequest,
	PatientHandoverData,
	GetHandoverActionItemsResponse,
} from "../types";
import type { SituationAwarenessStatus } from "@/types/domain";

// ========================================
// QUERY KEYS
// ========================================

export const handoverQueryKeys = {
	all: ["handovers"] as const,
	lists: () => [...handoverQueryKeys.all, "list"] as const,
	list: (filters: { page?: number; pageSize?: number; userId?: string; status?: string }) =>
		[...handoverQueryKeys.lists(), filters] as const,
	details: () => [...handoverQueryKeys.all, "detail"] as const,
	detail: (id: string) => [...handoverQueryKeys.details(), id] as const,
	messages: (id: string) => [...handoverQueryKeys.detail(id), "messages"] as const,
	activity: (id: string) => [...handoverQueryKeys.detail(id), "activity"] as const,
	checklists: (id: string) => [...handoverQueryKeys.detail(id), "checklists"] as const,
	contingencyPlans: (id: string) => [...handoverQueryKeys.detail(id), "contingency-plans"] as const,
	situationAwareness: (id: string) => [...handoverQueryKeys.detail(id), "situation-awareness"] as const,
    patientData: (id: string) => [...handoverQueryKeys.detail(id), "patient-data"] as const,
    patientHandoverData: (id: string) => [...handoverQueryKeys.detail(id), "patient-handover-data"] as const,
    synthesis: (id: string) => [...handoverQueryKeys.detail(id), "synthesis"] as const,
};

// ========================================
// API FUNCTIONS
// ========================================

// MAIN HANDOVER DATA
// ----------------------------------------

export async function getHandovers(
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<PaginatedHandovers> {
	const { data } = await api.get<PaginatedHandovers>("/me/handovers", { params: parameters });
	return data;
}

export async function getHandover(
	handoverId: string
): Promise<Handover> {
	const { data } = await api.get<Handover>(`/handovers/${handoverId}`);
	return data;
}

export async function getPatientHandoverData(
	handoverId: string
): Promise<PatientHandoverData> {
	const { data } = await api.get<PatientHandoverData>(`/handovers/${handoverId}/patient`);
	return data;
}

export async function createHandover(
	request: {
		patientId: string;
		fromDoctorId: string;
		toDoctorId: string;
		fromShiftId: string;
		toShiftId: string;
		initiatedBy: string;
		notes?: string;
	}
): Promise<Handover> {
	const { data } = await api.post<Handover>("/handovers", request);
	return data;
}

// HANDOVER STATE TRANSITIONS
// ----------------------------------------

export async function readyHandover(
	handoverId: string
): Promise<void> {
	await api.post(`/handovers/${handoverId}/ready`);
}

export async function startHandover(
	handoverId: string
): Promise<void> {
	await api.post(`/handovers/${handoverId}/start`);
}

export async function acceptHandover(
	handoverId: string
): Promise<void> {
	await api.post(`/handovers/${handoverId}/accept`);
}

export async function completeHandover(
	handoverId: string
): Promise<void> {
	await api.post(`/handovers/${handoverId}/complete`);
}

export async function cancelHandover(
	handoverId: string
): Promise<void> {
	await api.post(`/handovers/${handoverId}/cancel`);
}

export async function rejectHandover(
	handoverId: string,
	reason: string
): Promise<void> {
	await api.post(`/handovers/${handoverId}/reject`, { reason });
}


// PENDING HANDOVERS
// ----------------------------------------

export async function getPendingHandovers(
	userId: string
): Promise<{ handovers: Array<Handover> }> {
	const { data } = await api.get<{ handovers: Array<Handover> }>("/handovers/pending", { params: { userId } });
	return data;
}


// HANDOVER SECTIONS
// ----------------------------------------
// Removed getPatientData - data now consolidated into getPatientHandoverData

export async function getSynthesis(
	handoverId: string
): Promise<SynthesisResponse> {
	const { data } = await api.get<SynthesisResponse>(`/handovers/${handoverId}/synthesis`);
	return data;
}

// HANDOVER MESSAGES
// ----------------------------------------

export async function getHandoverMessages(
	handoverId: string
): Promise<Array<HandoverMessage>> {
	const { data } = await api.get<{messages: Array<HandoverMessage>}>(`/me/handovers/${handoverId}/messages`);
	return data.messages;
}

export async function createHandoverMessage(
	handoverId: string,
	messageText: string,
	messageType: "message" | "system" | "notification" = "message"
): Promise<{ success: boolean; message: HandoverMessage }> {
	const { data } = await api.post<{ success: boolean; message: HandoverMessage }>(`/me/handovers/${handoverId}/messages`, { messageText, messageType });
	return data;
}

// HANDOVER ACTIVITY LOG
// ----------------------------------------

export async function getHandoverActivityLog(
	handoverId: string
): Promise<Array<HandoverActivityItem>> {
	const { data } = await api.get<Array<HandoverActivityItem>>(`/me/handovers/${handoverId}/activity`);
	return data;
}

// HANDOVER CHECKLISTS
// ----------------------------------------

export async function getHandoverChecklists(
	handoverId: string
): Promise<Array<HandoverChecklistItem>> {
	const { data } = await api.get<Array<HandoverChecklistItem>>(`/me/handovers/${handoverId}/checklists`);
	return data;
}

export async function updateChecklistItem(
	handoverId: string,
	itemId: string,
	isChecked: boolean
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.put<{ success: boolean; message: string }>(`/me/handovers/${handoverId}/checklists/${itemId}`, { isChecked });
	return data;
}

// HANDOVER ACTION ITEMS
// ----------------------------------------

export async function getHandoverActionItems(
	handoverId: string
): Promise<GetHandoverActionItemsResponse> {
	const { data } = await api.get<GetHandoverActionItemsResponse>(`/me/handovers/${handoverId}/action-items`);
	return data;
}

export async function createActionItem(
	handoverId: string,
	description: string,
	priority: "low" | "medium" | "high" = "medium",
	dueTime?: string
): Promise<{ success: boolean; actionItemId: string }> {
	const { data } = await api.post<{ success: boolean; actionItemId: string }>(`/me/handovers/${handoverId}/action-items`, { description, priority, dueTime });
	return data;
}

export async function updateActionItem(
	handoverId: string,
	actionItemId: string,
	updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string }
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.put<{ success: boolean; message: string }>(`/me/handovers/${handoverId}/action-items/${actionItemId}`, updates);
	return data;
}

export async function deleteActionItem(
	handoverId: string,
	actionItemId: string
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.delete<{ success: boolean; message: string }>(`/me/handovers/${handoverId}/action-items/${actionItemId}`);
	return data;
}

// HANDOVER CONTINGENCY PLANS
// ----------------------------------------

/**
 * Retrieves contingency plans for a handover using the authenticated /me/ endpoint.
 * V3 Migration: Uses /me/handovers/{id}/contingency-plans and extracts 'contingencyPlans' from response.
 */
export async function getHandoverContingencyPlans(
	handoverId: string
): Promise<Array<HandoverContingencyPlan>> {
	const { data } = await api.get<ContingencyPlansResponse>(`/me/handovers/${handoverId}/contingency-plans`);
	return data.contingencyPlans;
}

/**
 * Creates a new contingency plan using the authenticated /me/ endpoint.
 * V3 Migration: Uses /me/handovers/{id}/contingency-plans and returns 'contingencyPlan' object.
 */
export async function createContingencyPlan(
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: "low" | "medium" | "high" = "medium"
): Promise<{ success: boolean; contingencyPlan: HandoverContingencyPlan }> {
	const { data } = await api.post<{ success: boolean; contingencyPlan: HandoverContingencyPlan }>(
		`/me/handovers/${handoverId}/contingency-plans`, 
		{ conditionText, actionText, priority }
	);
	return data;
}

// ========================================
// REACT QUERY HOOKS
// ========================================

// HOOKS: MAIN HANDOVER DATA
// ----------------------------------------

export function useHandovers(parameters?: {
	page?: number;
	pageSize?: number;
}): ReturnType<typeof useQuery<PaginatedHandovers | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.list({ ...parameters }),
		queryFn: () => getHandovers(parameters),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

export function useHandover(handoverId: string): ReturnType<typeof useQuery<Handover | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId),
		queryFn: () => getHandover(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

export function useCreateHandover(): ReturnType<typeof useMutation<Handover, Error, Parameters<typeof createHandover>[0]>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: (request) => createHandover(request),
		onSuccess: () => {
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.lists() });
		},
	});
}

// HOOKS: HANDOVER STATE TRANSITIONS
// ----------------------------------------

function useHandoverStateMutation(
	mutationFn: (
		handoverId: string
	) => Promise<void>
): ReturnType<typeof useMutation<void, Error, string>> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: (handoverId: string) => mutationFn(handoverId),
		onSuccess: () => {
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
		},
	});
}

export function useReadyHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useHandoverStateMutation(readyHandover);
}

export function useStartHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useHandoverStateMutation(startHandover);
}

export function useAcceptHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useHandoverStateMutation(acceptHandover);
}

export function useCompleteHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useHandoverStateMutation(completeHandover);
}

export function useCancelHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useHandoverStateMutation(cancelHandover);
}

export function useRejectHandover(): ReturnType<typeof useMutation<void, Error, { handoverId: string; reason: string }>> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: ({ handoverId, reason }: { handoverId: string; reason: string }) =>
			rejectHandover(handoverId, reason),
		onSuccess: () => {
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
		},
	});
}



// HOOKS: PENDING HANDOVERS
// ----------------------------------------

export function usePendingHandovers(userId: string): ReturnType<typeof useQuery<{ handovers: Array<Handover> } | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.list({ userId, status: "pending" }),
		queryFn: () => getPendingHandovers(userId),
		enabled: !!userId,
		staleTime: 30 * 1000, // 30 seconds
	});
}

// HOOKS: HANDOVER SECTIONS
// ----------------------------------------

// HOOKS: HANDOVER MESSAGES
// ----------------------------------------

export function useHandoverMessages(handoverId: string): ReturnType<typeof useQuery<Array<HandoverMessage> | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.messages(handoverId),
		queryFn: () => getHandoverMessages(handoverId),
		enabled: !!handoverId,
		staleTime: 30 * 1000, // 30 seconds
	});
}

export function useCreateHandoverMessage(): ReturnType<typeof useMutation<{ success: boolean; message: HandoverMessage }, Error, { handoverId: string; messageText: string; messageType?: "message" | "system" | "notification" }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			messageText,
			messageType,
		}: {
			handoverId: string;
			messageText: string;
			messageType?: "message" | "system" | "notification";
		}) => createHandoverMessage(handoverId, messageText, messageType),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.messages(variables.handoverId),
			});
		},
	});
}

// HOOKS: HANDOVER ACTIVITY LOG
// ----------------------------------------

export function useHandoverActivityLog(handoverId: string): ReturnType<typeof useQuery<Array<HandoverActivityItem> | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.activity(handoverId),
		queryFn: () => getHandoverActivityLog(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

// HOOKS: HANDOVER CHECKLISTS
// ----------------------------------------

export function useHandoverChecklists(handoverId: string): ReturnType<typeof useQuery<Array<HandoverChecklistItem> | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.checklists(handoverId),
		queryFn: () => getHandoverChecklists(handoverId),
		enabled: !!handoverId,
		staleTime: 2 * 60 * 1000, // 2 minutes
	});
}

export function useUpdateChecklistItem(): ReturnType<typeof useMutation<{ success: boolean; message: string }, Error, { handoverId: string; itemId: string; isChecked: boolean }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			itemId,
			isChecked,
		}: {
			handoverId: string;
			itemId: string;
			isChecked: boolean;
		}) => updateChecklistItem(handoverId, itemId, isChecked),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.checklists(variables.handoverId),
			});
		},
	});
}

// HOOKS: HANDOVER ACTION ITEMS
// ----------------------------------------

export function useHandoverActionItems(handoverId: string): ReturnType<typeof useQuery<GetHandoverActionItemsResponse | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId).concat("action-items"),
		queryFn: () => getHandoverActionItems(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateActionItem(): ReturnType<typeof useMutation<{ success: boolean; actionItemId: string }, Error, { handoverId: string; description: string; priority?: "low" | "medium" | "high"; dueTime?: string }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			description,
			priority,
			dueTime,
		}: {
			handoverId: string;
			description: string;
			priority?: "low" | "medium" | "high";
			dueTime?: string;
		}) => createActionItem(handoverId, description, priority, dueTime),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

export function useUpdateActionItem(): ReturnType<typeof useMutation<{ success: boolean; message: string }, Error, { handoverId: string; actionItemId: string; updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string } }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			actionItemId,
			updates,
		}: {
			handoverId: string;
			actionItemId: string;
			updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string };
		}) => updateActionItem(handoverId, actionItemId, updates),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

export function useDeleteActionItem(): ReturnType<typeof useMutation<{ success: boolean; message: string }, Error, { handoverId: string; actionItemId: string }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			actionItemId,
		}: {
			handoverId: string;
			actionItemId: string;
		}) => deleteActionItem(handoverId, actionItemId),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

// HOOKS: HANDOVER CONTINGENCY PLANS
// ----------------------------------------

export function useHandoverContingencyPlans(handoverId: string): ReturnType<typeof useQuery<Array<HandoverContingencyPlan> | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.contingencyPlans(handoverId),
		queryFn: () => getHandoverContingencyPlans(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateContingencyPlan(): ReturnType<typeof useMutation<{ success: boolean; contingencyPlan: HandoverContingencyPlan }, Error, { handoverId: string; conditionText: string; actionText: string; priority?: "low" | "medium" | "high" }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			conditionText,
			actionText,
			priority,
		}: {
			handoverId: string;
			conditionText: string;
			actionText: string;
			priority?: "low" | "medium" | "high";
		}) => createContingencyPlan(handoverId, conditionText, actionText, priority),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}

// ========================================
// SITUATION AWARENESS API FUNCTIONS
// ========================================

/**
 * Retrieves situation awareness data.
 * V3 Migration: Returns 'situationAwareness' object instead of 'section'.
 */
export async function getSituationAwareness(
	handoverId: string
): Promise<SituationAwarenessResponse> {
	const { data } = await api.get<SituationAwarenessResponse>(`/handovers/${handoverId}/situation-awareness`);
	return data;
}

export async function updatePatientData(
	handoverId: string,
	request: UpdatePatientDataRequest
): Promise<ApiResponse<void>> {
	const { data } = await api.put<ApiResponse<void>>(`/handovers/${handoverId}/patient-data`, request);
	return data;
}

/**
 * Updates situation awareness data.
 * V3 Migration: Requires 'status' field in request.
 */
export async function updateSituationAwareness(
	handoverId: string,
	request: UpdateSituationAwarenessRequest
): Promise<ApiResponse<void>> {
	const { data } = await api.put<ApiResponse<void>>(`/handovers/${handoverId}/situation-awareness`, request);
	return data;
}

export async function updateSynthesis(
	handoverId: string,
	request: { content?: string; status: string }
): Promise<ApiResponse<void>> {
	const { data } = await api.put<ApiResponse<void>>(`/handovers/${handoverId}/synthesis`, request);
	return data;
}

export async function deleteContingencyPlan(
	handoverId: string,
	contingencyId: string
): Promise<void> {
	await api.delete(`/handovers/${handoverId}/contingency-plans/${contingencyId}`);
}

// ========================================
// SITUATION AWARENESS HOOKS
// ========================================

export function useSituationAwareness(handoverId: string): ReturnType<typeof useQuery<SituationAwarenessResponse | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.situationAwareness(handoverId),
		queryFn: () => getSituationAwareness(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

// Removed usePatientData - data now consolidated into usePatientHandoverData

export function useSynthesis(handoverId: string): ReturnType<typeof useQuery<SynthesisResponse | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.synthesis(handoverId),
		queryFn: () => getSynthesis(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useUpdatePatientData(): ReturnType<typeof useMutation<ApiResponse<void>, Error, { handoverId: string } & UpdatePatientDataRequest>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({ handoverId, ...request }: { handoverId: string } & UpdatePatientDataRequest) =>
			updatePatientData(handoverId, request),
		onSuccess: (_, { handoverId }) => {
			return queryClient.invalidateQueries({ queryKey: handoverQueryKeys.patientData(handoverId) });
		},
	});
}

export function useUpdateSituationAwareness(): ReturnType<typeof useMutation<ApiResponse<void>, Error, { handoverId: string; content: string; status?: SituationAwarenessStatus }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			content,
			status,
		}: {
			handoverId: string;
			content: string;
			status?: SituationAwarenessStatus;
		}) =>
			// eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
			updateSituationAwareness(handoverId, { content, status: status ?? "Draft" }),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.situationAwareness(variables.handoverId),
			});
		},
	});
}

export function useUpdateSynthesis(): ReturnType<typeof useMutation<ApiResponse<void>, Error, { handoverId: string; content?: string; status: string }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			content,
			status,
		}: {
			handoverId: string;
			content?: string;
			status: string;
		}) => updateSynthesis(handoverId, { content, status }),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.synthesis(variables.handoverId),
			});
		},
	});
}

export function useDeleteContingencyPlan(): ReturnType<typeof useMutation<void, Error, { handoverId: string; contingencyId: string }>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			contingencyId,
		}: {
			handoverId: string;
			contingencyId: string;
		}) => deleteContingencyPlan(handoverId, contingencyId),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}

export function usePatientHandoverData(handoverId: string): ReturnType<typeof useQuery<PatientHandoverData | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.patientHandoverData(handoverId),
		queryFn: () => getPatientHandoverData(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

// ========================================
// HELPER FUNCTIONS
// ========================================
