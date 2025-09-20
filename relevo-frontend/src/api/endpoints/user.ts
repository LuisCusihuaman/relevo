import { useQuery } from "@tanstack/react-query";
import { api } from "../client";
import type { User } from "../types";

// Query Keys for cache invalidation
export const userQueryKeys = {
	all: ["user"] as const,
	profile: () => [...userQueryKeys.all, "profile"] as const,
};

/**
 * Get current user profile
 */
export async function getCurrentUserProfile(): Promise<User> {
	const { data } = await api.get<User>("/me/profile");
	return data;
}

/**
 * Hook to get current user profile
 */
export function useCurrentUserProfile(): ReturnType<typeof useQuery<User | undefined, Error>> {
	return useQuery({
		queryKey: userQueryKeys.profile(),
		queryFn: () => getCurrentUserProfile(),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: User | undefined) => data,
	});
}
