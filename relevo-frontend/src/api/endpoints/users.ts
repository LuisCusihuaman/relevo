/**
 * Users API endpoints
 * Rule: Concise-FP - Functional, no classes
 */
import { useQuery } from "@tanstack/react-query";
import { api } from "@/api/client";

export type User = {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	fullName: string;
};

export type GetAllUsersResponse = {
	users: Array<User>;
};

export const userQueryKeys = {
	all: ["users"] as const,
	allUsers: () => [...userQueryKeys.all, "list"] as const,
};

/**
 * Get all users
 */
export async function getAllUsers(): Promise<Array<User>> {
	const { data } = await api.get<GetAllUsersResponse>("/users");
	return (data.users ?? []).map((u) => ({
		id: u.id,
		email: u.email,
		firstName: u.firstName,
		lastName: u.lastName,
		fullName: u.fullName,
	}));
}

/**
 * Hook to get all users
 */
export function useAllUsers(): ReturnType<
	typeof useQuery<Array<User> | undefined, Error>
> {
	return useQuery({
		queryKey: userQueryKeys.allUsers(),
		queryFn: () => getAllUsers(),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
	});
}

