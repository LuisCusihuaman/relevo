import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthenticatedApi } from "@/hooks/useAuthenticatedApi";
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
	SituationAwarenessStatus,
} from "../types";

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
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<PaginatedHandovers> {
	const data = await authenticatedApiCall<PaginatedHandovers>({
		method: "GET",
		url: "/me/handovers",
		params: parameters,
	});
	return data;
}

export async function getHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<Handover> {
	const data = await authenticatedApiCall<Handover>({
		method: "GET",
		url: `/handovers/${handoverId}`,
	});
	return data;
}

export async function getPatientHandoverData(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<PatientHandoverData> {
	const data = await authenticatedApiCall<PatientHandoverData>({
		method: "GET",
		url: `/handovers/${handoverId}/patient`,
	});
	return data;
}

export async function createHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
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
	const data = await authenticatedApiCall<Handover>({
		method: "POST",
		url: "/handovers",
		data: request,
	});
	return data;
}

// HANDOVER STATE TRANSITIONS
// ----------------------------------------

export async function readyHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: `/handovers/${handoverId}/ready`,
	});
}

export async function startHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: `/handovers/${handoverId}/start`,
	});
}

export async function acceptHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: `/handovers/${handoverId}/accept`,
	});
}

export async function completeHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: `/handovers/${handoverId}/complete`,
	});
}

export async function cancelHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: `/handovers/${handoverId}/cancel`,
	});
}

export async function rejectHandover(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	reason: string
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: `/handovers/${handoverId}/reject`,
		data: { reason },
	});
}


// PENDING HANDOVERS
// ----------------------------------------

export async function getPendingHandovers(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	userId: string
): Promise<{ handovers: Array<Handover> }> {
	const data = await authenticatedApiCall<{ handovers: Array<Handover> }>({
		method: "GET",
		url: `/handovers/pending`,
		params: { userId },
	});
	return data;
}


// HANDOVER SECTIONS
// ----------------------------------------
// Removed getPatientData - data now consolidated into getPatientHandoverData

export async function getSynthesis(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<SynthesisResponse> {
	const data = await authenticatedApiCall<SynthesisResponse>({
		method: "GET",
		url: `/handovers/${handoverId}/synthesis`,
	});
	return data;
}

// HANDOVER MESSAGES
// ----------------------------------------

export async function getHandoverMessages(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<Array<HandoverMessage>> {
	const data = await authenticatedApiCall<{messages: Array<HandoverMessage>}>({
		method: "GET",
		url: `/me/handovers/${handoverId}/messages`,
	});
	return data.messages;
}

export async function createHandoverMessage(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	messageText: string,
	messageType: "message" | "system" | "notification" = "message"
): Promise<{ success: boolean; message: HandoverMessage }> {
	const data = await authenticatedApiCall<{ success: boolean; message: HandoverMessage }>({
		method: "POST",
		url: `/me/handovers/${handoverId}/messages`,
		data: { messageText, messageType },
	});
	return data;
}

// HANDOVER ACTIVITY LOG
// ----------------------------------------

export async function getHandoverActivityLog(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<Array<HandoverActivityItem>> {
	const data = await authenticatedApiCall<Array<HandoverActivityItem>>({
		method: "GET",
		url: `/me/handovers/${handoverId}/activity`,
	});
	return data;
}

// HANDOVER CHECKLISTS
// ----------------------------------------

export async function getHandoverChecklists(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<Array<HandoverChecklistItem>> {
	const data = await authenticatedApiCall<Array<HandoverChecklistItem>>({
		method: "GET",
		url: `/me/handovers/${handoverId}/checklists`,
	});
	return data;
}

export async function updateChecklistItem(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	itemId: string,
	isChecked: boolean
): Promise<{ success: boolean; message: string }> {
	const data = await authenticatedApiCall<{ success: boolean; message: string }>({
		method: "PUT",
		url: `/me/handovers/${handoverId}/checklists/${itemId}`,
		data: { isChecked },
	});
	return data;
}

// HANDOVER ACTION ITEMS
// ----------------------------------------

export async function getHandoverActionItems(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<GetHandoverActionItemsResponse> {
	const data = await authenticatedApiCall<GetHandoverActionItemsResponse>({
		method: "GET",
		url: `/me/handovers/${handoverId}/action-items`,
	});
	return data;
}

export async function createActionItem(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	description: string,
	priority: "low" | "medium" | "high" = "medium",
	dueTime?: string
): Promise<{ success: boolean; actionItemId: string }> {
	const data = await authenticatedApiCall<{ success: boolean; actionItemId: string }>({
		method: "POST",
		url: `/me/handovers/${handoverId}/action-items`,
		data: { description, priority, dueTime },
	});
	return data;
}

export async function updateActionItem(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	actionItemId: string,
	updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string }
): Promise<{ success: boolean; message: string }> {
	const data = await authenticatedApiCall<{ success: boolean; message: string }>({
		method: "PUT",
		url: `/me/handovers/${handoverId}/action-items/${actionItemId}`,
		data: updates,
	});
	return data;
}

export async function deleteActionItem(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	actionItemId: string
): Promise<{ success: boolean; message: string }> {
	const data = await authenticatedApiCall<{ success: boolean; message: string }>({
		method: "DELETE",
		url: `/me/handovers/${handoverId}/action-items/${actionItemId}`,
	});
	return data;
}

// HANDOVER CONTINGENCY PLANS
// ----------------------------------------

/**
 * Retrieves contingency plans for a handover using the authenticated /me/ endpoint.
 * V3 Migration: Uses /me/handovers/{id}/contingency-plans and extracts 'contingencyPlans' from response.
 */
export async function getHandoverContingencyPlans(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<Array<HandoverContingencyPlan>> {
	const data = await authenticatedApiCall<ContingencyPlansResponse>({
		method: "GET",
		url: `/me/handovers/${handoverId}/contingency-plans`,
	});
	return data.contingencyPlans;
}

/**
 * Creates a new contingency plan using the authenticated /me/ endpoint.
 * V3 Migration: Uses /me/handovers/{id}/contingency-plans and returns 'contingencyPlan' object.
 */
export async function createContingencyPlan(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: "low" | "medium" | "high" = "medium"
): Promise<{ success: boolean; contingencyPlan: HandoverContingencyPlan }> {
	const data = await authenticatedApiCall<{ success: boolean; contingencyPlan: HandoverContingencyPlan }>({
		method: "POST",
		url: `/me/handovers/${handoverId}/contingency-plans`,
		data: { conditionText, actionText, priority },
	});
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
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.list({ ...parameters }),
		queryFn: () => getHandovers(authenticatedApiCall, parameters),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

export function useHandover(handoverId: string): ReturnType<typeof useQuery<Handover | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId),
		queryFn: () => getHandover(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

export function useCreateHandover(): ReturnType<typeof useMutation<Handover, Error, Parameters<typeof createHandover>[1]>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: (request) => createHandover(authenticatedApiCall, request),
		onSuccess: () => {
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.lists() });
		},
	});
}

// HOOKS: HANDOVER STATE TRANSITIONS
// ----------------------------------------

function useHandoverStateMutation(
	mutationFn: (
		authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
		handoverId: string
	) => Promise<void>
): ReturnType<typeof useMutation<void, Error, string>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useMutation({
		mutationFn: (handoverId: string) => mutationFn(authenticatedApiCall, handoverId),
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
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useMutation({
		mutationFn: ({ handoverId, reason }: { handoverId: string; reason: string }) =>
			rejectHandover(authenticatedApiCall, handoverId, reason),
		onSuccess: () => {
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
		},
	});
}



// HOOKS: PENDING HANDOVERS
// ----------------------------------------

export function usePendingHandovers(userId: string): ReturnType<typeof useQuery<{ handovers: Array<Handover> } | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.list({ userId, status: "pending" }),
		queryFn: () => getPendingHandovers(authenticatedApiCall, userId),
		enabled: !!userId,
		staleTime: 30 * 1000, // 30 seconds
	});
}

// HOOKS: HANDOVER SECTIONS
// ----------------------------------------

// HOOKS: HANDOVER MESSAGES
// ----------------------------------------

export function useHandoverMessages(handoverId: string): ReturnType<typeof useQuery<Array<HandoverMessage> | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useQuery({
		queryKey: handoverQueryKeys.messages(handoverId),
		queryFn: () => getHandoverMessages(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 30 * 1000, // 30 seconds
	});
}

export function useCreateHandoverMessage(): ReturnType<typeof useMutation<{ success: boolean; message: HandoverMessage }, Error, { handoverId: string; messageText: string; messageType?: "message" | "system" | "notification" }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			messageText,
			messageType,
		}: {
			handoverId: string;
			messageText: string;
			messageType?: "message" | "system" | "notification";
		}) => createHandoverMessage(authenticatedApiCall, handoverId, messageText, messageType),
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
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.activity(handoverId),
		queryFn: () => getHandoverActivityLog(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

// HOOKS: HANDOVER CHECKLISTS
// ----------------------------------------

export function useHandoverChecklists(handoverId: string): ReturnType<typeof useQuery<Array<HandoverChecklistItem> | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.checklists(handoverId),
		queryFn: () => getHandoverChecklists(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 2 * 60 * 1000, // 2 minutes
	});
}

export function useUpdateChecklistItem(): ReturnType<typeof useMutation<{ success: boolean; message: string }, Error, { handoverId: string; itemId: string; isChecked: boolean }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			itemId,
			isChecked,
		}: {
			handoverId: string;
			itemId: string;
			isChecked: boolean;
		}) => updateChecklistItem(authenticatedApiCall, handoverId, itemId, isChecked),
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
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId).concat("action-items"),
		queryFn: () => getHandoverActionItems(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateActionItem(): ReturnType<typeof useMutation<{ success: boolean; actionItemId: string }, Error, { handoverId: string; description: string; priority?: "low" | "medium" | "high"; dueTime?: string }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

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
		}) => createActionItem(authenticatedApiCall, handoverId, description, priority, dueTime),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

export function useUpdateActionItem(): ReturnType<typeof useMutation<{ success: boolean; message: string }, Error, { handoverId: string; actionItemId: string; updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string } }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			actionItemId,
			updates,
		}: {
			handoverId: string;
			actionItemId: string;
			updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string };
		}) => updateActionItem(authenticatedApiCall, handoverId, actionItemId, updates),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

export function useDeleteActionItem(): ReturnType<typeof useMutation<{ success: boolean; message: string }, Error, { handoverId: string; actionItemId: string }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			actionItemId,
		}: {
			handoverId: string;
			actionItemId: string;
		}) => deleteActionItem(authenticatedApiCall, handoverId, actionItemId),
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
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.contingencyPlans(handoverId),
		queryFn: () => getHandoverContingencyPlans(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateContingencyPlan(): ReturnType<typeof useMutation<{ success: boolean; contingencyPlan: HandoverContingencyPlan }, Error, { handoverId: string; conditionText: string; actionText: string; priority?: "low" | "medium" | "high" }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

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
		}) => createContingencyPlan(authenticatedApiCall, handoverId, conditionText, actionText, priority),
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
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<SituationAwarenessResponse> {
	const data = await authenticatedApiCall<SituationAwarenessResponse>({
		method: "GET",
		url: `/handovers/${handoverId}/situation-awareness`,
	});
	return data;
}

export async function updatePatientData(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	request: UpdatePatientDataRequest
): Promise<ApiResponse> {
	const data = await authenticatedApiCall<ApiResponse>({
		method: "PUT",
		url: `/handovers/${handoverId}/patient-data`,
		data: request,
	});
	return data;
}

/**
 * Updates situation awareness data.
 * V3 Migration: Requires 'status' field in request.
 */
export async function updateSituationAwareness(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	request: UpdateSituationAwarenessRequest
): Promise<ApiResponse> {
	const data = await authenticatedApiCall<ApiResponse>({
		method: "PUT",
		url: `/handovers/${handoverId}/situation-awareness`,
		data: request,
	});
	return data;
}

export async function updateSynthesis(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	request: { content?: string; status: string }
): Promise<ApiResponse> {
	const data = await authenticatedApiCall<ApiResponse>({
		method: "PUT",
		url: `/handovers/${handoverId}/synthesis`,
		data: request,
	});
	return data;
}

export async function deleteContingencyPlan(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string,
	contingencyId: string
): Promise<void> {
	await authenticatedApiCall({
		method: "DELETE",
		url: `/handovers/${handoverId}/contingency-plans/${contingencyId}`,
	});
}

// ========================================
// SITUATION AWARENESS HOOKS
// ========================================

export function useSituationAwareness(handoverId: string): ReturnType<typeof useQuery<SituationAwarenessResponse | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.situationAwareness(handoverId),
		queryFn: () => getSituationAwareness(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

// Removed usePatientData - data now consolidated into usePatientHandoverData

export function useSynthesis(handoverId: string): ReturnType<typeof useQuery<SynthesisResponse | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.synthesis(handoverId),
		queryFn: () => getSynthesis(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useUpdatePatientData(): ReturnType<typeof useMutation<ApiResponse, Error, { handoverId: string } & UpdatePatientDataRequest>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({ handoverId, ...request }: { handoverId: string } & UpdatePatientDataRequest) =>
			updatePatientData(authenticatedApiCall, handoverId, request),
		onSuccess: (_, { handoverId }) => {
			return queryClient.invalidateQueries({ queryKey: handoverQueryKeys.patientData(handoverId) });
		},
	});
}

export function useUpdateSituationAwareness(): ReturnType<typeof useMutation<ApiResponse, Error, { handoverId: string; content: string; status?: SituationAwarenessStatus }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			content,
			status = "Draft",
		}: {
			handoverId: string;
			content: string;
			status?: SituationAwarenessStatus;
		}) => updateSituationAwareness(authenticatedApiCall, handoverId, { content, status }),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.situationAwareness(variables.handoverId),
			});
		},
	});
}

export function useUpdateSynthesis(): ReturnType<typeof useMutation<ApiResponse, Error, { handoverId: string; content?: string; status: string }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			content,
			status,
		}: {
			handoverId: string;
			content?: string;
			status: string;
		}) => updateSynthesis(authenticatedApiCall, handoverId, { content, status }),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.synthesis(variables.handoverId),
			});
		},
	});
}

export function useDeleteContingencyPlan(): ReturnType<typeof useMutation<void, Error, { handoverId: string; contingencyId: string }>> {
	const queryClient = useQueryClient();
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: ({
			handoverId,
			contingencyId,
		}: {
			handoverId: string;
			contingencyId: string;
		}) => deleteContingencyPlan(authenticatedApiCall, handoverId, contingencyId),
		onSuccess: (_data, variables) => {
			void queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}

export function usePatientHandoverData(handoverId: string): ReturnType<typeof useQuery<PatientHandoverData | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: handoverQueryKeys.patientHandoverData(handoverId),
		queryFn: () => getPatientHandoverData(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

// ========================================
// HELPER FUNCTIONS
// ========================================
