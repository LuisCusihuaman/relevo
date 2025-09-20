import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../client";
import type { ActiveHandoverData, HandoverSection } from "../types";

// Query Keys for cache invalidation
export const handoverQueryKeys = {
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
		queryKey: handoverQueryKeys.active,
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
			queryClient.invalidateQueries({ queryKey: handoverQueryKeys.active });
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

	// Parse action items from content (assuming comma-separated format)
	return actionItemsSection.content
		.split(",")
		.map((item, index) => ({
			id: `action-${index}`,
			description: item.trim(),
			isCompleted: false, // This would come from HANDOVER_ACTION_ITEMS table in a real implementation
		}));
}
