import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../client";
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
	CreateContingencyPlanRequest,
	UpdateSituationAwarenessRequest,
	ApiResponse,
	PatientDataResponse,
	SynthesisResponse,
	UpdatePatientDataRequest,
} from "../types";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

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
    synthesis: (id: string) => [...handoverQueryKeys.detail(id), "synthesis"] as const,
};

// ========================================
// API FUNCTIONS
// ========================================

// MAIN HANDOVER DATA
// ----------------------------------------

export async function getHandovers(parameters?: {
	page?: number;
	pageSize?: number;
}): Promise<PaginatedHandovers> {
	const { data } = await api.get<PaginatedHandovers>("/me/handovers", { params: parameters });
	return data;
}

export async function getHandover(handoverId: string): Promise<Handover> {
	const { data } = await api.get<Handover>(`/handovers/${handoverId}`);
	return data;
}

export async function createHandover(request: {
	patientId: string;
	fromDoctorId: string;
	toDoctorId: string;
	fromShiftId: string;
	toShiftId: string;
	initiatedBy: string;
	notes?: string;
}): Promise<Handover> {
	const { data } = await api.post<Handover>("/handovers", request);
	return data;
}

// HANDOVER STATE TRANSITIONS
// ----------------------------------------

export async function readyHandover(handoverId: string): Promise<void> {
	await api.post(`/handovers/${handoverId}/ready`);
}

export async function startHandover(handoverId: string): Promise<void> {
	await api.post(`/handovers/${handoverId}/start`);
}

export async function acceptHandover(handoverId: string): Promise<void> {
	await api.post(`/handovers/${handoverId}/accept`);
}

export async function completeHandover(handoverId: string): Promise<void> {
	await api.post(`/handovers/${handoverId}/complete`);
}

export async function cancelHandover(handoverId: string): Promise<void> {
	await api.post(`/handovers/${handoverId}/cancel`);
}

export async function rejectHandover(handoverId: string, reason: string): Promise<void> {
	await api.post(`/handovers/${handoverId}/reject`, { reason });
}


// PENDING HANDOVERS
// ----------------------------------------

export async function getPendingHandovers(userId: string): Promise<{ handovers: Handover[] }> {
	const { data } = await api.get<{ handovers: Handover[] }>(`/handovers/pending?userId=${userId}`);
	return data;
}


// HANDOVER SECTIONS
// ----------------------------------------

export async function getPatientData(handoverId: string): Promise<PatientDataResponse> {
	const { data } = await api.get<PatientDataResponse>(`/handovers/${handoverId}/patient-data`);
	return data;
}

export async function getSynthesis(handoverId: string): Promise<SynthesisResponse> {
    const { data } = await api.get<SynthesisResponse>(`/handovers/${handoverId}/synthesis`);
    return data;
}

// HANDOVER MESSAGES
// ----------------------------------------

export async function getHandoverMessages(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<HandoverMessage[]> {
	const data = await authenticatedApiCall<{messages: HandoverMessage[]}>({
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

export async function getHandoverActivityLog(handoverId: string): Promise<HandoverActivityItem[]> {
	const { data } = await api.get<HandoverActivityItem[]>(`/me/handovers/${handoverId}/activity`);
	return data;
}

// HANDOVER CHECKLISTS
// ----------------------------------------

export async function getHandoverChecklists(handoverId: string): Promise<HandoverChecklistItem[]> {
	const { data } = await api.get<HandoverChecklistItem[]>(`/me/handovers/${handoverId}/checklists`);
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

// HANDOVER ACTION ITEMS
// ----------------------------------------

export async function getHandoverActionItems(handoverId: string): Promise<{
	actionItems: Array<{
		id: string;
		handoverId: string;
		description: string;
		isCompleted: boolean;
		createdAt: string;
		updatedAt: string;
		completedAt: string | null;
	}>;
}> {
	const { data } = await api.get<{
		actionItems: Array<{
			id: string;
			handoverId: string;
			description: string;
			isCompleted: boolean;
			createdAt: string;
			updatedAt: string;
			completedAt: string | null;
		}>;
	}>(`/me/handovers/${handoverId}/action-items`);
	return data;
}

export async function createActionItem(
	handoverId: string,
	description: string,
	priority: "low" | "medium" | "high" = "medium",
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
	updates: { description?: string; isCompleted?: boolean; priority?: "low" | "medium" | "high"; dueTime?: string }
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

// HANDOVER CONTINGENCY PLANS
// ----------------------------------------

export async function getHandoverContingencyPlans(handoverId: string): Promise<HandoverContingencyPlan[]> {
	const { data } = await api.get<HandoverContingencyPlan[]>(`/me/handovers/${handoverId}/contingency-plans`);
	return data;
}

export async function createContingencyPlan(
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: "low" | "medium" | "high" = "medium"
): Promise<{ success: boolean; message: HandoverContingencyPlan }> {
	const { data } = await api.post<{ success: boolean; message: HandoverContingencyPlan }>(
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

export function useCreateHandover() {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: createHandover,
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: handoverQueryKeys.lists() });
		},
	});
}

// HOOKS: HANDOVER STATE TRANSITIONS
// ----------------------------------------

function useHandoverStateMutation(mutationFn: (handoverId: string) => Promise<void>) {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn,
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
		},
	});
}

export function useReadyHandover() {
	return useHandoverStateMutation(readyHandover);
}

export function useStartHandover() {
	return useHandoverStateMutation(startHandover);
}

export function useAcceptHandover() {
	return useHandoverStateMutation(acceptHandover);
}

export function useCompleteHandover() {
	return useHandoverStateMutation(completeHandover);
}

export function useCancelHandover() {
	return useHandoverStateMutation(cancelHandover);
}

export function useRejectHandover() {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: ({ handoverId, reason }: { handoverId: string; reason: string }) => rejectHandover(handoverId, reason),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: handoverQueryKeys.all });
		},
	});
}



// HOOKS: PENDING HANDOVERS
// ----------------------------------------

export function usePendingHandovers(userId: string) {
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

export function useHandoverMessages(handoverId: string) {
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useQuery({
		queryKey: handoverQueryKeys.messages(handoverId),
		queryFn: () => getHandoverMessages(authenticatedApiCall, handoverId),
		enabled: !!handoverId,
		staleTime: 30 * 1000, // 30 seconds
	});
}

export function useCreateHandoverMessage() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.messages(variables.handoverId),
			});
		},
	});
}

// HOOKS: HANDOVER ACTIVITY LOG
// ----------------------------------------

export function useHandoverActivityLog(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.activity(handoverId),
		queryFn: () => getHandoverActivityLog(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

// HOOKS: HANDOVER CHECKLISTS
// ----------------------------------------

export function useHandoverChecklists(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.checklists(handoverId),
		queryFn: () => getHandoverChecklists(handoverId),
		enabled: !!handoverId,
		staleTime: 2 * 60 * 1000, // 2 minutes
	});
}

export function useUpdateChecklistItem() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.checklists(variables.handoverId),
			});
		},
	});
}

// HOOKS: HANDOVER ACTION ITEMS
// ----------------------------------------

export function useHandoverActionItems(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.detail(handoverId).concat("action-items"),
		queryFn: () => getHandoverActionItems(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateActionItem() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

export function useUpdateActionItem() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

export function useDeleteActionItem() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.detail(variables.handoverId),
			});
		},
	});
}

// HOOKS: HANDOVER CONTINGENCY PLANS
// ----------------------------------------

export function useHandoverContingencyPlans(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.contingencyPlans(handoverId),
		queryFn: () => getHandoverContingencyPlans(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateContingencyPlan() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}

// ========================================
// SITUATION AWARENESS API FUNCTIONS
// ========================================

export async function getSituationAwareness(handoverId: string): Promise<SituationAwarenessResponse> {
	const { data } = await api.get<SituationAwarenessResponse>(`/handovers/${handoverId}/situation-awareness`);
	return data;
}

export async function updatePatientData(handoverId: string, request: UpdatePatientDataRequest): Promise<ApiResponse> {
    const { data } = await api.put<ApiResponse>(`/handovers/${handoverId}/patient-data`, request);
    return data;
}

export async function updateSituationAwareness(handoverId: string, request: UpdateSituationAwarenessRequest): Promise<ApiResponse> {
	const { data } = await api.put<ApiResponse>(`/handovers/${handoverId}/situation-awareness`, request);
	return data;
}

export async function updateSynthesis(handoverId: string, request: { content?: string; status: string }): Promise<ApiResponse> {
    const { data } = await api.put<ApiResponse>(`/handovers/${handoverId}/synthesis`, request);
    return data;
}

export async function getContingencyPlans(handoverId: string): Promise<ContingencyPlansResponse> {
	const { data } = await api.get<ContingencyPlansResponse>(`/handovers/${handoverId}/contingency-plans`);
	return data;
}

export async function createHandoverContingencyPlan(
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: "low" | "medium" | "high" = "medium"
): Promise<HandoverContingencyPlan> {
	const request: CreateContingencyPlanRequest = {
		conditionText,
		actionText,
		priority,
	};
	const { data } = await api.post<HandoverContingencyPlan>(`/handovers/${handoverId}/contingency-plans`, request);
	return data;
}

export async function deleteContingencyPlan(handoverId: string, contingencyId: string): Promise<ApiResponse> {
	const { data } = await api.delete<ApiResponse>(`/handovers/${handoverId}/contingency-plans/${contingencyId}`);
	return data;
}

// ========================================
// SITUATION AWARENESS HOOKS
// ========================================

export function useSituationAwareness(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.situationAwareness(handoverId),
		queryFn: () => getSituationAwareness(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function usePatientData(handoverId: string) {
    return useQuery({
        queryKey: handoverQueryKeys.patientData(handoverId),
        queryFn: () => getPatientData(handoverId),
        enabled: !!handoverId,
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
}

export function useSynthesis(handoverId: string) {
    return useQuery({
        queryKey: handoverQueryKeys.synthesis(handoverId),
        queryFn: () => getSynthesis(handoverId),
        enabled: !!handoverId,
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
}

export function useUpdatePatientData() {
    const queryClient = useQueryClient();
    const { t } = useTranslation();

    return useMutation({
        mutationFn: ({ handoverId, ...request }: { handoverId: string } & UpdatePatientDataRequest) =>
            updatePatientData(handoverId, request),
        onSuccess: (_, { handoverId }) => {
            toast.success(t("api.handovers.updatePatientData.success"));
            return queryClient.invalidateQueries({ queryKey: handoverQueryKeys.patientData(handoverId) });
        },
        onError: () => {
            toast.error(t("api.handovers.updatePatientData.error"));
        },
    });
}

export function useUpdateSituationAwareness() {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			content,
		}: {
			handoverId: string;
			content: string;
		}) => updateSituationAwareness(handoverId, { content }),
		onSuccess: (_data, variables) => {
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.situationAwareness(variables.handoverId),
			});
		},
	});
}

export function useUpdateSynthesis() {
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
            queryClient.invalidateQueries({
                queryKey: handoverQueryKeys.synthesis(variables.handoverId),
            });
        },
    });
}

export function useContingencyPlans(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.contingencyPlans(handoverId),
		queryFn: () => getContingencyPlans(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

export function useCreateHandoverContingencyPlan() {
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
		}) => createHandoverContingencyPlan(handoverId, conditionText, actionText, priority),
		onSuccess: (_data, variables) => {
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}

export function useDeleteContingencyPlan() {
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
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}

// ========================================
// HELPER FUNCTIONS
// ========================================
