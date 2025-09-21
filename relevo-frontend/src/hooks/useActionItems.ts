import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createActionItem, deleteActionItem, getHandoverActionItems } from "@/api/endpoints/handovers";
import { api } from "@/api";

// API response type
interface HandoverActionItemResponse {
  id: string;
  handoverId: string;
  description: string;
  isCompleted: boolean;
  createdAt: string;
  updatedAt: string;
  completedAt: string | null;
}

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
  initialActionItems?: Array<ActionItem>;
}

// Helper function to transform API response to ActionItem
const transformActionItem = (item: HandoverActionItemResponse): ActionItem => ({
  id: item.id,
  description: item.description,
  priority: "medium" as const,
  isCompleted: item.isCompleted,
  submittedBy: "Current User",
  submittedTime: new Date(item.createdAt).toLocaleTimeString(),
  submittedDate: new Date(item.createdAt).toLocaleDateString(),
  shift: "Current Shift",
});

// Query configuration
const actionItemsQueryConfig = (
  handoverId?: string,
  initialActionItems: Array<ActionItem> = []
): {
  queryKey: Array<string | undefined>;
  queryFn: () => Promise<Array<ActionItem>>;
  enabled: boolean;
  staleTime: number;
  gcTime: number;
} => ({
  queryKey: ["actionItems", handoverId],
  queryFn: async (): Promise<Array<ActionItem>> => {
    if (!handoverId) return initialActionItems;

    const response = await getHandoverActionItems(handoverId);
    return response.actionItems.map(transformActionItem);
  },
  enabled: !!handoverId,
  staleTime: 1000 * 60 * 5, // 5 minutes
  gcTime: 1000 * 60 * 10, // 10 minutes
});

// Mutation configurations
const createActionItemMutationConfig = (
  queryClient: ReturnType<typeof useQueryClient>,
  handoverId?: string
): {
  mutationFn: (data: {
    description: string;
    priority?: "low" | "medium" | "high";
    dueTime?: string;
  }) => Promise<{ success: boolean; actionItemId: string }>;
  onSuccess: () => void;
} => ({
  mutationFn: async (data: {
    description: string;
    priority?: "low" | "medium" | "high";
    dueTime?: string;
  }): Promise<{ success: boolean; actionItemId: string }> => {
    if (!handoverId) throw new Error("Handover ID is required");
    return createActionItem(handoverId, data.description, data.priority, data.dueTime);
  },
  onSuccess: (): void => {
    void queryClient.invalidateQueries({ queryKey: ["actionItems", handoverId] });
  },
});

const updateActionItemMutationConfig = (
  queryClient: ReturnType<typeof useQueryClient>,
  handoverId?: string
): {
  mutationFn: (data: {
    actionItemId: string;
    updates: {
      description?: string;
      isCompleted?: boolean;
      priority?: "low" | "medium" | "high";
      dueTime?: string;
    };
  }) => Promise<{ success: boolean }>;
  onSuccess: () => void;
} => ({
  mutationFn: async (data: {
    actionItemId: string;
    updates: {
      description?: string;
      isCompleted?: boolean;
      priority?: "low" | "medium" | "high";
      dueTime?: string;
    };
  }): Promise<{ success: boolean }> => {
    if (!handoverId) throw new Error("Handover ID is required");
    if (data.updates.isCompleted !== undefined) {
      const response = await api.put<{ success: boolean }>(
        `/me/handovers/${handoverId}/action-items/${data.actionItemId}`,
        { isCompleted: data.updates.isCompleted }
      );
      return response.data;
    }
    throw new Error("Only completion status updates are supported");
  },
  onSuccess: (): void => {
    void queryClient.invalidateQueries({ queryKey: ["actionItems", handoverId] });
  },
});

const deleteActionItemMutationConfig = (
  queryClient: ReturnType<typeof useQueryClient>,
  handoverId?: string
): {
  mutationFn: (actionItemId: string) => Promise<{ success: boolean; message: string }>;
  onSuccess: () => void;
} => ({
  mutationFn: async (actionItemId: string): Promise<{ success: boolean; message: string }> => {
    if (!handoverId) throw new Error("Handover ID is required");
    return deleteActionItem(handoverId, actionItemId);
  },
  onSuccess: (): void => {
    void queryClient.invalidateQueries({ queryKey: ["actionItems", handoverId] });
  },
});

export function useActionItems({ handoverId, initialActionItems = [] }: UseActionItemsProps = {}): {
  actionItems: Array<ActionItem>;
  isLoading: boolean;
  error: Error | null;
  createActionItem: (data: {
    description: string;
    priority?: "low" | "medium" | "high";
    dueTime?: string;
  }) => Promise<{ success: boolean; actionItemId: string }>;
  updateActionItem: (data: {
    actionItemId: string;
    updates: {
      description?: string;
      isCompleted?: boolean;
      priority?: "low" | "medium" | "high";
      dueTime?: string;
    };
  }) => Promise<{ success: boolean }>;
  deleteActionItem: (actionItemId: string) => Promise<{ success: boolean; message: string }>;
  isCreating: boolean;
  isUpdating: boolean;
  isDeleting: boolean;
} {
  const queryClient = useQueryClient();

  // Simplified query and mutations using helper configs
  const actionItemsQuery = useQuery(actionItemsQueryConfig(handoverId, initialActionItems));
  const createMutation = useMutation(createActionItemMutationConfig(queryClient, handoverId));
  const updateMutation = useMutation(updateActionItemMutationConfig(queryClient, handoverId));
  const deleteMutation = useMutation(deleteActionItemMutationConfig(queryClient, handoverId));

  return {
    actionItems: actionItemsQuery.data || ([] as Array<ActionItem>),
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
