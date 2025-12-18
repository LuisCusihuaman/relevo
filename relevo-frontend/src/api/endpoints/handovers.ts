import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../client";
import type { Schemas } from "@/api/generated";
import type {
	HandoverSummary,
	HandoverDetail,
	ContingencyPlan,
	PatientHandoverData,
	HandoverActionItem,
	SituationAwarenessStatus,
	IllnessSeverity,
} from "@/types/domain";
import {
	mapApiHandoverRecord,
	mapApiHandoverDto,
	mapApiHandoverDetail,
	mapApiContingencyPlan,
	mapApiPatientHandoverData,
} from "@/api/mappers";

// ========================================
// TYPES
// ========================================

type PaginatedHandovers = {
	items: Array<HandoverSummary>;
	pagination: Schemas["PaginationInfo"];
};

type SituationAwarenessResponse = Schemas["GetSituationAwarenessResponse"];
type SynthesisResponse = Schemas["GetSynthesisResponse"];
type UpdateSituationAwarenessRequest = Schemas["UpdateSituationAwarenessRequest"];

type UpdatePatientDataRequest = {
	illnessSeverity: IllnessSeverity;
	summaryText?: string;
};

type HandoverMessage = {
	id: string;
	handoverId: string;
	userId: string;
	userName: string;
	messageText: string;
	messageType: "message" | "system" | "notification";
	createdAt: string;
	updatedAt: string;
};

type HandoverActivityItem = {
	id: string;
	handoverId: string;
	userId: string;
	userName: string;
	activityType: string;
	activityDescription?: string;
	sectionAffected?: string;
	metadata?: Record<string, unknown>;
	createdAt: string;
};

type HandoverChecklistItem = {
	id: string;
	handoverId: string;
	userId: string;
	itemId: string;
	itemCategory: string;
	itemLabel: string;
	itemDescription?: string;
	isRequired: boolean;
	isChecked: boolean;
	checkedAt?: string;
	createdAt: string;
};

type ApiResponse<T> = { success: boolean; message: string; data?: T };
type ActionItemsResponse = { actionItems: Array<HandoverActionItem> };
type Priority = "low" | "medium" | "high";

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

export async function getHandovers(parameters?: { page?: number; pageSize?: number }): Promise<PaginatedHandovers> {
	const { data } = await api.get<Schemas["GetMyHandoversResponse"]>("/me/handovers", { params: parameters });
	return {
		items: (data.items ?? []).map(mapApiHandoverRecord),
		pagination: data.pagination ?? { totalItems: 0, currentPage: 1, pageSize: 10, totalPages: 0 },
	};
}

export async function getHandover(handoverId: string): Promise<HandoverDetail> {
	const { data } = await api.get<Schemas["GetHandoverByIdResponse"]>(`/handovers/${handoverId}`);
	return mapApiHandoverDetail(data);
}

export async function getPatientHandoverData(handoverId: string): Promise<PatientHandoverData> {
	const { data } = await api.get<Schemas["GetPatientHandoverDataResponse"]>(`/handovers/${handoverId}/patient`);
	return mapApiPatientHandoverData(data);
}

export async function createHandover(request: {
	patientId: string;
	fromDoctorId: string;
	toDoctorId: string;
	fromShiftId: string;
	toShiftId: string;
	initiatedBy: string;
	notes?: string;
}): Promise<HandoverDetail> {
	const { data } = await api.post<Schemas["GetHandoverByIdResponse"]>("/handovers", request);
	return mapApiHandoverDetail(data);
}

// State Transitions
export const readyHandover = (id: string): Promise<void> => api.post(`/handovers/${id}/ready`, {}).then(() => undefined);
export const startHandover = (id: string): Promise<void> => api.post(`/handovers/${id}/start`, {}).then(() => undefined);
export const acceptHandover = (id: string): Promise<void> => api.post(`/handovers/${id}/accept`, {}).then(() => undefined);
export const completeHandover = (id: string): Promise<void> =>
	api.post(`/handovers/${id}/complete`, {}).then(() => undefined);
export const cancelHandover = (id: string): Promise<void> => api.post(`/handovers/${id}/cancel`, {}).then(() => undefined);
export const rejectHandover = (id: string, reason: string): Promise<void> =>
	api.post(`/handovers/${id}/reject`, { reason }).then(() => undefined);

export async function getPendingHandovers(userId: string): Promise<{ handovers: Array<HandoverSummary> }> {
	const { data } = await api.get<Schemas["GetPendingHandoversResponse"]>("/handovers/pending", { params: { userId } });
	return { handovers: (data.handovers ?? []).map(mapApiHandoverDto) };
}

export async function getSynthesis(handoverId: string): Promise<SynthesisResponse> {
	const { data } = await api.get<SynthesisResponse>(`/handovers/${handoverId}/synthesis`);
	return data;
}

export async function getHandoverMessages(handoverId: string): Promise<Array<HandoverMessage>> {
	const { data } = await api.get<{ messages: Array<HandoverMessage> }>(`/me/handovers/${handoverId}/messages`);
	return data.messages;
}

export async function createHandoverMessage(
	handoverId: string,
	messageText: string,
	messageType: "message" | "system" | "notification" = "message"
): Promise<{ success: boolean; message: HandoverMessage }> {
	const { data } = await api.post<{ success: boolean; message: HandoverMessage }>(
		`/me/handovers/${handoverId}/messages`,
		{ messageText, messageType }
	);
	return data;
}

export async function getHandoverActivityLog(handoverId: string): Promise<Array<HandoverActivityItem>> {
	const { data } = await api.get<Array<HandoverActivityItem>>(`/me/handovers/${handoverId}/activity`);
	return data;
}

export async function getHandoverChecklists(handoverId: string): Promise<Array<HandoverChecklistItem>> {
	const { data } = await api.get<Array<HandoverChecklistItem>>(`/me/handovers/${handoverId}/checklists`);
	return data;
}

export async function updateChecklistItem(
	handoverId: string,
	itemId: string,
	isChecked: boolean
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.put<{ success: boolean; message: string }>(
		`/me/handovers/${handoverId}/checklists/${itemId}`,
		{ isChecked }
	);
	return data;
}

export async function getHandoverActionItems(handoverId: string): Promise<ActionItemsResponse> {
	const { data } = await api.get<ActionItemsResponse>(`/me/handovers/${handoverId}/action-items`);
	return data;
}

export async function createActionItem(
	handoverId: string,
	description: string,
	priority: Priority = "medium",
	dueTime?: string
): Promise<{ success: boolean; actionItemId: string }> {
	const { data } = await api.post<{ success: boolean; actionItemId: string }>(
		`/me/handovers/${handoverId}/action-items`,
		{ description, priority, dueTime }
	);
	return data;
}

export async function updateActionItem(
	handoverId: string,
	actionItemId: string,
	updates: { description?: string; isCompleted?: boolean; priority?: Priority; dueTime?: string }
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.put<{ success: boolean; message: string }>(
		`/me/handovers/${handoverId}/action-items/${actionItemId}`,
		updates
	);
	return data;
}

export async function deleteActionItem(
	handoverId: string,
	actionItemId: string
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.delete<{ success: boolean; message: string }>(
		`/me/handovers/${handoverId}/action-items/${actionItemId}`
	);
	return data;
}

export async function getHandoverContingencyPlans(handoverId: string): Promise<Array<ContingencyPlan>> {
	const { data } = await api.get<Schemas["GetMeContingencyPlansResponse"]>(
		`/me/handovers/${handoverId}/contingency-plans`
	);
	return (data.contingencyPlans ?? []).map(mapApiContingencyPlan);
}

export async function createContingencyPlan(
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: Priority = "medium"
): Promise<{ success: boolean; contingencyPlan: ContingencyPlan | null }> {
	const { data } = await api.post<Schemas["CreateMeContingencyPlanResponse"]>(
		`/me/handovers/${handoverId}/contingency-plans`,
		{ conditionText, actionText, priority }
	);
	return {
		success: data.success ?? false,
		contingencyPlan: data.contingencyPlan ? mapApiContingencyPlan(data.contingencyPlan) : null,
	};
}

export async function getSituationAwareness(handoverId: string): Promise<SituationAwarenessResponse> {
	const { data } = await api.get<SituationAwarenessResponse>(`/handovers/${handoverId}/situation-awareness`);
	return data;
}

export async function updatePatientData(
	handoverId: string,
	request: UpdatePatientDataRequest
): Promise<ApiResponse<void>> {
	console.log("[API] updatePatientData called", { handoverId, request });
	try {
		const { data } = await api.put<ApiResponse<void>>(`/handovers/${handoverId}/patient-data`, request);
		console.log("[API] updatePatientData response", data);
		return data;
	} catch (error) {
		console.error("[API] updatePatientData error", error);
		throw error;
	}
}

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

export async function deleteContingencyPlan(handoverId: string, contingencyId: string): Promise<void> {
	await api.delete(`/handovers/${handoverId}/contingency-plans/${contingencyId}`);
}

// ========================================
// REACT QUERY HOOKS
// ========================================

const STALE_TIME_SHORT = 30 * 1000;
const STALE_TIME_MEDIUM = 2 * 60 * 1000;
const STALE_TIME_LONG = 5 * 60 * 1000;
const GC_TIME = 10 * 60 * 1000;

// Helper for state transition mutations
function useStateMutation(
	fn: (id: string) => Promise<void>
): ReturnType<typeof useMutation<void, Error, string>> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: fn,
		onSuccess: () => void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all }),
	});
}

// Helper for invalidating handover-related queries
function useInvalidatingMutation<TData, TVariables extends { handoverId: string }>(
	mutationFn: (variables: TVariables) => Promise<TData>,
	getQueryKey: (variables: TVariables) => ReadonlyArray<unknown>
): ReturnType<typeof useMutation<TData, Error, TVariables>> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: (variables) => {
			console.log("[useInvalidatingMutation] Calling mutationFn", variables);
			return mutationFn(variables);
		},
		onSuccess: (_, variables) => {
			console.log("[useInvalidatingMutation] onSuccess", variables);
			void queryClient.invalidateQueries({ queryKey: getQueryKey(variables) });
		},
		onError: (error, variables) => {
			console.error("[useInvalidatingMutation] onError", error, variables);
		},
	});
}

// Main Handover Data
export function useHandovers(
	parameters?: { page?: number; pageSize?: number }
): ReturnType<typeof useQuery<PaginatedHandovers, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.list({ ...parameters }),
		queryFn: () => getHandovers(parameters),
		staleTime: STALE_TIME_LONG,
		gcTime: GC_TIME,
	});
}

export function useHandover(handoverId: string): ReturnType<typeof useQuery<HandoverDetail, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId),
		queryFn: () => getHandover(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
		gcTime: GC_TIME,
	});
}

export function usePatientHandoverData(
	handoverId: string
): ReturnType<typeof useQuery<PatientHandoverData, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.patientHandoverData(handoverId),
		queryFn: () => getPatientHandoverData(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
		gcTime: GC_TIME,
	});
}

export function useCreateHandover(): ReturnType<
	typeof useMutation<HandoverDetail, Error, Parameters<typeof createHandover>[0]>
> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: createHandover,
		onSuccess: () => void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.lists() }),
	});
}

// State Transitions
export function useReadyHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useStateMutation(readyHandover);
}
export function useStartHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useStateMutation(startHandover);
}
export function useAcceptHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useStateMutation(acceptHandover);
}
export function useCompleteHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useStateMutation(completeHandover);
}
export function useCancelHandover(): ReturnType<typeof useMutation<void, Error, string>> {
	return useStateMutation(cancelHandover);
}

export function useRejectHandover(): ReturnType<
	typeof useMutation<void, Error, { handoverId: string; reason: string }>
> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: ({ handoverId, reason }: { handoverId: string; reason: string }) =>
			rejectHandover(handoverId, reason),
		onSuccess: () => void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all }),
	});
}

// Pending Handovers
export function usePendingHandovers(
	userId: string
): ReturnType<typeof useQuery<{ handovers: Array<HandoverSummary> }, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.list({ userId, status: "pending" }),
		queryFn: () => getPendingHandovers(userId),
		enabled: !!userId,
		staleTime: STALE_TIME_SHORT,
	});
}

// Messages
export function useHandoverMessages(
	handoverId: string
): ReturnType<typeof useQuery<Array<HandoverMessage>, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.messages(handoverId),
		queryFn: () => getHandoverMessages(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_SHORT,
	});
}

type CreateMessageVariables = {
	handoverId: string;
	messageText: string;
	messageType?: "message" | "system" | "notification";
};

export function useCreateHandoverMessage(): ReturnType<
	typeof useMutation<{ success: boolean; message: HandoverMessage }, Error, CreateMessageVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, messageText, messageType = "message" }: CreateMessageVariables) =>
			createHandoverMessage(handoverId, messageText, messageType),
		(variables) => handoverQueryKeys.messages(variables.handoverId)
	);
}

// Activity Log
export function useHandoverActivityLog(
	handoverId: string
): ReturnType<typeof useQuery<Array<HandoverActivityItem>, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.activity(handoverId),
		queryFn: () => getHandoverActivityLog(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
	});
}

// Checklists
export function useHandoverChecklists(
	handoverId: string
): ReturnType<typeof useQuery<Array<HandoverChecklistItem>, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.checklists(handoverId),
		queryFn: () => getHandoverChecklists(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_MEDIUM,
	});
}

type UpdateChecklistVariables = { handoverId: string; itemId: string; isChecked: boolean };

export function useUpdateChecklistItem(): ReturnType<
	typeof useMutation<{ success: boolean; message: string }, Error, UpdateChecklistVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, itemId, isChecked }: UpdateChecklistVariables) =>
			updateChecklistItem(handoverId, itemId, isChecked),
		(variables) => handoverQueryKeys.checklists(variables.handoverId)
	);
}

// Action Items
export function useHandoverActionItems(
	handoverId: string
): ReturnType<typeof useQuery<ActionItemsResponse, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId).concat("action-items"),
		queryFn: () => getHandoverActionItems(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
	});
}

type CreateActionItemVariables = {
	handoverId: string;
	description: string;
	priority?: Priority;
	dueTime?: string;
};

export function useCreateActionItem(): ReturnType<
	typeof useMutation<{ success: boolean; actionItemId: string }, Error, CreateActionItemVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, description, priority, dueTime }: CreateActionItemVariables) =>
			createActionItem(handoverId, description, priority, dueTime),
		(variables) => handoverQueryKeys.detail(variables.handoverId)
	);
}

type UpdateActionItemVariables = {
	handoverId: string;
	actionItemId: string;
	updates: { description?: string; isCompleted?: boolean; priority?: Priority; dueTime?: string };
};

export function useUpdateActionItem(): ReturnType<
	typeof useMutation<{ success: boolean; message: string }, Error, UpdateActionItemVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, actionItemId, updates }: UpdateActionItemVariables) =>
			updateActionItem(handoverId, actionItemId, updates),
		(variables) => handoverQueryKeys.detail(variables.handoverId)
	);
}

type DeleteActionItemVariables = { handoverId: string; actionItemId: string };

export function useDeleteActionItem(): ReturnType<
	typeof useMutation<{ success: boolean; message: string }, Error, DeleteActionItemVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, actionItemId }: DeleteActionItemVariables) => deleteActionItem(handoverId, actionItemId),
		(variables) => handoverQueryKeys.detail(variables.handoverId)
	);
}

// Contingency Plans
export function useHandoverContingencyPlans(
	handoverId: string
): ReturnType<typeof useQuery<Array<ContingencyPlan>, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.contingencyPlans(handoverId),
		queryFn: () => getHandoverContingencyPlans(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
	});
}

type CreateContingencyPlanVariables = {
	handoverId: string;
	conditionText: string;
	actionText: string;
	priority?: Priority;
};

export function useCreateContingencyPlan(): ReturnType<
	typeof useMutation<
		{ success: boolean; contingencyPlan: ContingencyPlan | null },
		Error,
		CreateContingencyPlanVariables
	>
> {
	return useInvalidatingMutation(
		({ handoverId, conditionText, actionText, priority }: CreateContingencyPlanVariables) =>
			createContingencyPlan(handoverId, conditionText, actionText, priority),
		(variables) => handoverQueryKeys.contingencyPlans(variables.handoverId)
	);
}

type DeleteContingencyPlanVariables = { handoverId: string; contingencyId: string };

export function useDeleteContingencyPlan(): ReturnType<
	typeof useMutation<void, Error, DeleteContingencyPlanVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, contingencyId }: DeleteContingencyPlanVariables) =>
			deleteContingencyPlan(handoverId, contingencyId),
		(variables) => handoverQueryKeys.contingencyPlans(variables.handoverId)
	);
}

// Situation Awareness & Synthesis
export function useSituationAwareness(
	handoverId: string
): ReturnType<typeof useQuery<SituationAwarenessResponse, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.situationAwareness(handoverId),
		queryFn: () => getSituationAwareness(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
	});
}

export function useSynthesis(handoverId: string): ReturnType<typeof useQuery<SynthesisResponse, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.synthesis(handoverId),
		queryFn: () => getSynthesis(handoverId),
		enabled: !!handoverId,
		staleTime: STALE_TIME_LONG,
	});
}

type UpdatePatientDataVariables = { handoverId: string } & UpdatePatientDataRequest;

export function useUpdatePatientData(): ReturnType<
	typeof useMutation<ApiResponse<void>, Error, UpdatePatientDataVariables>
> {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: ({ handoverId, ...request }: UpdatePatientDataVariables) => {
			console.log("[useUpdatePatientData] Calling mutationFn", { handoverId, request });
			return updatePatientData(handoverId, request);
		},
		onSuccess: (_, variables) => {
			console.log("[useUpdatePatientData] onSuccess, updating cache", variables);
			// Update patientHandoverData cache optimistically
			const patientDataKey = handoverQueryKeys.patientHandoverData(variables.handoverId);
			const now = new Date().toISOString();
			queryClient.setQueryData<PatientHandoverData>(patientDataKey, (oldData) => {
				if (!oldData) return oldData;
				return {
					...oldData,
					illnessSeverity: variables.illnessSeverity,
					summaryText: variables.summaryText ?? oldData.summaryText,
					updatedAt: now, // Update timestamp to current time
				};
			});
			// Also invalidate to ensure fresh data from server
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.patientData(variables.handoverId) });
			void queryClient.invalidateQueries({ queryKey: handoverQueryKeys.patientHandoverData(variables.handoverId) });
		},
		onError: (error, variables) => {
			console.error("[useUpdatePatientData] onError", error, variables);
		},
	});
}

type UpdateSituationAwarenessVariables = {
	handoverId: string;
	content: string;
	status?: SituationAwarenessStatus;
};

export function useUpdateSituationAwareness(): ReturnType<
	typeof useMutation<ApiResponse<void>, Error, UpdateSituationAwarenessVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, content, status }: UpdateSituationAwarenessVariables) =>
			updateSituationAwareness(handoverId, { content, status: status ?? "Draft" }),
		(variables) => handoverQueryKeys.situationAwareness(variables.handoverId)
	);
}

type UpdateSynthesisVariables = { handoverId: string; content?: string; status: string };

export function useUpdateSynthesis(): ReturnType<
	typeof useMutation<ApiResponse<void>, Error, UpdateSynthesisVariables>
> {
	return useInvalidatingMutation(
		({ handoverId, content, status }: UpdateSynthesisVariables) =>
			updateSynthesis(handoverId, { content, status }),
		(variables) => handoverQueryKeys.synthesis(variables.handoverId)
	);
}
