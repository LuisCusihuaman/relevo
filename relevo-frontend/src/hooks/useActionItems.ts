import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createActionItem, deleteActionItem, updateActionItem } from "@/api/endpoints/handover";

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
      if (!handoverId) return initialActionItems;
      // In a real implementation, this would fetch from the API
      return initialActionItems;
    },
    enabled: !!handoverId,
    initialData: initialActionItems,
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
      return updateActionItem(handoverId, data.actionItemId, data.updates);
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
