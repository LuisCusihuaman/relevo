import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createActionItem, deleteActionItem, getHandoverActionItems } from "@/api/endpoints/handover";
import { api } from "@/api";

export interface ActionItem {
  id: string;
  description: string;
  priority: "low" | "medium" | "high";
  dueTime?: string;
  isCompleted: boolean;
  submittedBy: string;
  submittedTime: string;
  submittedDate: string;
  shift: string;
}

export interface UseActionItemsProps {
  handoverId?: string;
  initialActionItems?: ActionItem[];
}

export function useActionItems({ handoverId, initialActionItems = [] }: UseActionItemsProps = {}) {
  const queryClient = useQueryClient();

  // Query for action items (if handoverId is provided)
  const actionItemsQuery = useQuery({
    queryKey: ["actionItems", handoverId],
    queryFn: async () => {
      if (!handoverId) {
        return initialActionItems;
      }

      try {
        const response = await getHandoverActionItems(handoverId);

        const transformedItems = response.actionItems.map(item => ({
          id: item.id,
          description: item.description,
          priority: "medium" as const, // Default priority since API doesn't return it
          isCompleted: item.isCompleted,
          submittedBy: "Current User", // Default since API doesn't return it
          submittedTime: new Date(item.createdAt).toLocaleTimeString(),
          submittedDate: new Date(item.createdAt).toLocaleDateString(),
          shift: "Current Shift", // Default since API doesn't return it
        }));

        return transformedItems;
      } catch (error) {
        console.error("Error loading action items:", error);
        return initialActionItems;
      }
    },
    enabled: !!handoverId,
    // Remove initialData to force fresh data on mount
    // initialData: initialActionItems,
    staleTime: 1000 * 60 * 5, // 5 minutes
    gcTime: 1000 * 60 * 10, // 10 minutes
  });

  // Create action item mutation
  const createMutation = useMutation({
    mutationFn: async (data: {
      description: string;
      priority?: "low" | "medium" | "high";
      dueTime?: string;
    }) => {
      if (!handoverId) throw new Error("Handover ID is required");
      return createActionItem(handoverId, data.description, data.priority, data.dueTime);
    },
    onSuccess: () => {
      // Invalidate and refetch action items
      queryClient.invalidateQueries({ queryKey: ["actionItems", handoverId] });
    },
  });

  // Update action item mutation
  const updateMutation = useMutation({
    mutationFn: async (data: {
      actionItemId: string;
      updates: {
        description?: string;
        isCompleted?: boolean;
        priority?: "low" | "medium" | "high";
        dueTime?: string;
      };
    }) => {
      if (!handoverId) throw new Error("Handover ID is required");
      // For now, only support updating isCompleted status
      if (data.updates.isCompleted !== undefined) {
        // Use the PUT endpoint for updating completion status
        const { data: response } = await api.put<{ success: boolean }>(
          `/me/handovers/${handoverId}/action-items/${data.actionItemId}`,
          { isCompleted: data.updates.isCompleted }
        );
        return response;
      }
      throw new Error("Only completion status updates are supported");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["actionItems", handoverId] });
    },
  });

  // Delete action item mutation
  const deleteMutation = useMutation({
    mutationFn: async (actionItemId: string) => {
      if (!handoverId) throw new Error("Handover ID is required");
      return deleteActionItem(handoverId, actionItemId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["actionItems", handoverId] });
    },
  });

  return {
    actionItems: actionItemsQuery.data || [],
    isLoading: actionItemsQuery.isLoading,
    error: actionItemsQuery.error,
    createActionItem: createMutation.mutateAsync,
    updateActionItem: updateMutation.mutateAsync,
    deleteActionItem: deleteMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
