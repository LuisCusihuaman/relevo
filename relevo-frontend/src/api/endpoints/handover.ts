import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../client";
import type { ActiveHandoverData, HandoverSection } from "../types";

// Query Keys for cache invalidation
export const activeHandoverQueryKeys = {
	active: ["handover", "active"] as const,
	sections: (handoverId: string) => ["handover", "sections", handoverId] as const,
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
 * Create a new action item
 */
export async function createActionItem(
	handoverId: string,
	description: string,
	priority: "low" | "medium" | "high" = "medium",
	dueTime?: string
): Promise<{ success: boolean; message: string; actionItem?: any }> {
	const { data } = await api.post<{ success: boolean; message: string; actionItem?: any }>(
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
