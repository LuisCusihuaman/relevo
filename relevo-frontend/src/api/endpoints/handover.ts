import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../client";
import type {
	ActiveHandoverData,
	HandoverSection,
	HandoverMessage,
	HandoverActivityItem,
	HandoverChecklistItem,
	HandoverContingencyPlan
} from "../types";

// Query Keys for cache invalidation
export const activeHandoverQueryKeys = {
	active: ["handover", "active"] as const,
	sections: (handoverId: string) => ["handover", "sections", handoverId] as const,
	pending: (userId: string) => ["handovers", "pending", userId] as const,
};

/**
 * Get active handover for current user
 */
export async function getActiveHandover(): Promise<ActiveHandoverData> {
	const { data } = await api.get<ActiveHandoverData>("/me/handovers/active");
	return data;
}

/**
 * Update a handover section
 */
export async function updateHandoverSection(
	handoverId: string,
	sectionId: string,
	content: string,
	status: string
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.put<{ success: boolean; message: string }>(
		`/me/handovers/${handoverId}/sections/${sectionId}`,
		{ content, status }
	);
	return data;
}

/**
 * Hook to get active handover
 */
export function useActiveHandover(): ReturnType<typeof useQuery<ActiveHandoverData | undefined, Error>> {
	return useQuery({
		queryKey: activeHandoverQueryKeys.active,
		queryFn: () => getActiveHandover(),
		staleTime: 30 * 1000, // 30 seconds
		gcTime: 5 * 60 * 1000, // 5 minutes
		select: (data: ActiveHandoverData | undefined) => data,
	});
}

/**
 * Hook to update handover section
 */
export function useUpdateHandoverSection() {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({
			handoverId,
			sectionId,
			content,
			status,
		}: {
			handoverId: string;
			sectionId: string;
			content: string;
			status: string;
		}) => updateHandoverSection(handoverId, sectionId, content, status),
		onSuccess: () => {
			// Invalidate and refetch active handover data
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

/**
 * Get a specific section by type from handover data
 */
export function getSectionByType(sections: HandoverSection[], sectionType: string): HandoverSection | undefined {
	return sections.find(section => section.sectionType === sectionType);
}

/**
 * Get all action items from handover data
 */
export function getActionItems(sections: HandoverSection[]): Array<{ id: string; description: string; isCompleted: boolean }> {
	const actionItemsSection = getSectionByType(sections, "action_items");
	if (!actionItemsSection?.content) return [];

	// Parse action items from content (assuming JSON format or comma-separated)
	try {
		// Try to parse as JSON first
		const parsed = JSON.parse(actionItemsSection.content);
		if (Array.isArray(parsed)) {
			return parsed.map((item: any, index) => ({
				id: item.id || `action-${index}`,
				description: item.description || item.task || item,
				isCompleted: item.isCompleted || item.completed || false,
			}));
		}
	} catch {
		// Fallback to comma-separated format
		return actionItemsSection.content
			.split(",")
			.map((item, index) => ({
				id: `action-${index}`,
				description: item.trim(),
				isCompleted: false,
			}));
	}

	return [];
}

/**
 * Get handover action items for a specific handover
 */
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

/**
 * Create a new action item
 */
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

/**
 * Update an action item
 */
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

/**
 * Delete an action item
 */
export async function deleteActionItem(
	handoverId: string,
	actionItemId: string
): Promise<{ success: boolean; message: string }> {
	const { data } = await api.delete<{ success: boolean; message: string }>(
		`/me/handovers/${handoverId}/action-items/${actionItemId}`
	);
	return data;
}

/**
 * Get handover messages for a specific handover
 */
export async function getHandoverMessages(handoverId: string): Promise<HandoverMessage[]> {
	const { data } = await api.get<HandoverMessage[]>(`/me/handovers/${handoverId}/messages`);
	return data;
}

/**
 * Create a new handover message
 */
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

/**
 * Get handover activity history
 */
export async function getHandoverActivityLog(handoverId: string): Promise<HandoverActivityItem[]> {
	const { data } = await api.get<HandoverActivityItem[]>(`/me/handovers/${handoverId}/activity`);
	return data;
}

/**
 * Get handover checklists
 */
export async function getHandoverChecklists(handoverId: string): Promise<HandoverChecklistItem[]> {
	const { data } = await api.get<HandoverChecklistItem[]>(`/me/handovers/${handoverId}/checklists`);
	return data;
}

/**
 * Update checklist item status
 */
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

/**
 * Get handover contingency plans
 */
export async function getHandoverContingencyPlans(handoverId: string): Promise<HandoverContingencyPlan[]> {
	const { data } = await api.get<HandoverContingencyPlan[]>(`/me/handovers/${handoverId}/contingency-plans`);
	return data;
}

/**
 * Create a new contingency plan
 */
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

/**
 * Hook to get handover messages
 */
export function useHandoverMessages(handoverId: string): ReturnType<typeof useQuery<HandoverMessage[] | undefined, Error>> {
	return useQuery({
		queryKey: [...activeHandoverQueryKeys.active, "messages", handoverId],
		queryFn: () => getHandoverMessages(handoverId),
		enabled: !!handoverId,
		staleTime: 30 * 1000, // 30 seconds
	});
}

/**
 * Hook to create handover messages
 */
export function useCreateHandoverMessage() {
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
			// Invalidate messages for this handover
			queryClient.invalidateQueries({
				queryKey: [...activeHandoverQueryKeys.active, "messages", variables.handoverId]
			});
		},
	});
}

/**
 * Hook to get handover activity log
 */
export function useHandoverActivityLog(handoverId: string): ReturnType<typeof useQuery<HandoverActivityItem[] | undefined, Error>> {
	return useQuery({
		queryKey: [...activeHandoverQueryKeys.active, "activity", handoverId],
		queryFn: () => getHandoverActivityLog(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

/**
 * Hook to get handover checklists
 */
export function useHandoverChecklists(handoverId: string): ReturnType<typeof useQuery<HandoverChecklistItem[] | undefined, Error>> {
	return useQuery({
		queryKey: [...activeHandoverQueryKeys.active, "checklists", handoverId],
		queryFn: () => getHandoverChecklists(handoverId),
		enabled: !!handoverId,
		staleTime: 2 * 60 * 1000, // 2 minutes
	});
}

/**
 * Hook to update checklist items
 */
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
			// Invalidate checklists for this handover
			queryClient.invalidateQueries({
				queryKey: [...activeHandoverQueryKeys.active, "checklists", variables.handoverId]
			});
		},
	});
}

/**
 * Hook to get handover contingency plans
 */
export function useHandoverContingencyPlans(handoverId: string): ReturnType<typeof useQuery<HandoverContingencyPlan[] | undefined, Error>> {
	return useQuery({
		queryKey: [...activeHandoverQueryKeys.active, "contingency-plans", handoverId],
		queryFn: () => getHandoverContingencyPlans(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
	});
}

/**
 * Hook to create contingency plans
 */
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
			// Invalidate contingency plans for this handover
			queryClient.invalidateQueries({
				queryKey: [...activeHandoverQueryKeys.active, "contingency-plans", variables.handoverId]
			});
		},
	});
}

// ========================================
// HANDOVER CREATION AND MANAGEMENT
// ========================================

/**
 * Create a new handover for shift transition
 */
export async function createHandover(request: {
	patientId: string;
	fromDoctorId: string;
	toDoctorId: string;
	fromShiftId: string;
	toShiftId: string;
	initiatedBy: string;
	notes?: string;
}): Promise<{
	id: string;
	patientId: string;
	patientName: string | null;
	status: string;
	fromDoctorId: string;
	toDoctorId: string;
	fromShiftId: string;
	toShiftId: string;
	createdAt: string | null;
}> {
	const { data } = await api.post<{
		id: string;
		patientId: string;
		patientName: string | null;
		status: string;
		fromDoctorId: string;
		toDoctorId: string;
		fromShiftId: string;
		toShiftId: string;
		createdAt: string | null;
	}>("/handovers", request);
	return data;
}

/**
 * Accept a handover (receiving doctor)
 */
export async function acceptHandover(handoverId: string): Promise<{
	success: boolean;
	handoverId: string;
	message: string;
}> {
	const { data } = await api.post<{
		success: boolean;
		handoverId: string;
		message: string;
	}>(`/handovers/${handoverId}/accept`);
	return data;
}

/**
 * Complete a handover (receiving doctor)
 */
export async function completeHandover(handoverId: string): Promise<{
	success: boolean;
	handoverId: string;
	message: string;
}> {
	const { data } = await api.post<{
		success: boolean;
		handoverId: string;
		message: string;
	}>(`/handovers/${handoverId}/complete`);
	return data;
}

/**
 * Get pending handovers for a user (receiving doctor)
 */
export async function getPendingHandovers(userId: string): Promise<{
	handovers: Array<{
		id: string;
		patientId: string;
		patientName: string | null;
		status: string;
		illnessSeverity: string;
		shiftName: string;
		createdAt: string | null;
		createdBy: string;
	}>;
}> {
	const { data } = await api.get<{
		handovers: Array<{
			id: string;
			patientId: string;
			patientName: string | null;
			status: string;
			illnessSeverity: string;
			shiftName: string;
			createdAt: string | null;
			createdBy: string;
		}>;
	}>(`/handovers/pending?userId=${userId}`);
	return data;
}

/**
 * Hook to create a new handover
 */
export function useCreateHandover() {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: (request: {
			patientId: string;
			fromDoctorId: string;
			toDoctorId: string;
			fromShiftId: string;
			toShiftId: string;
			initiatedBy: string;
			notes?: string;
		}) => createHandover(request),
		onSuccess: (_data, variables) => {
			// Invalidate pending handovers for the receiving doctor
			queryClient.invalidateQueries({
				queryKey: activeHandoverQueryKeys.pending(variables.toDoctorId)
			});
			// Also invalidate active handover queries
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

/**
 * Hook to accept a handover
 */
export function useAcceptHandover() {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({ handoverId }: { handoverId: string }) =>
			acceptHandover(handoverId),
		onSuccess: (_data, _variables) => {
			// Invalidate pending handovers and active handover
			queryClient.invalidateQueries({
				queryKey: activeHandoverQueryKeys.pending("current-user") // Invalidate for current user
			});
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

/**
 * Hook to complete a handover
 */
export function useCompleteHandover() {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: ({ handoverId }: { handoverId: string }) =>
			completeHandover(handoverId),
		onSuccess: (_data, _variables) => {
			// Invalidate pending handovers and active handover
			queryClient.invalidateQueries({
				queryKey: activeHandoverQueryKeys.pending("current-user") // Invalidate for current user
			});
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

// ========================================
// HANDOVER STATE TRANSITIONS
// ========================================

export async function readyHandover(handoverId: string): Promise<boolean> {
	const { data } = await api.post<boolean>(`/handovers/${handoverId}/ready`);
	return data;
}

export function useReadyHandover() {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: (handoverId: string) => readyHandover(handoverId),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

export async function startHandover(handoverId: string): Promise<boolean> {
	const { data } = await api.post<boolean>(`/handovers/${handoverId}/start`);
	return data;
}

export function useStartHandover() {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: (handoverId: string) => startHandover(handoverId),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

export async function cancelHandover(handoverId: string): Promise<boolean> {
	const { data } = await api.post<boolean>(`/handovers/${handoverId}/cancel`);
	return data;
}

export function useCancelHandover() {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: (handoverId: string) => cancelHandover(handoverId),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

export async function rejectHandover(handoverId: string, reason: string): Promise<boolean> {
	const { data } = await api.post<boolean>(`/handovers/${handoverId}/reject`, { reason });
	return data;
}

export function useRejectHandover() {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: ({ handoverId, reason }: { handoverId: string; reason: string }) => rejectHandover(handoverId, reason),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: activeHandoverQueryKeys.active });
		},
	});
}

/**
 * Hook to get pending handovers for a user
 */
export function usePendingHandovers(userId: string): ReturnType<typeof useQuery> {
	return useQuery({
		queryKey: activeHandoverQueryKeys.pending(userId),
		queryFn: () => getPendingHandovers(userId),
		enabled: !!userId,
		staleTime: 30 * 1000, // 30 seconds
	});
}
