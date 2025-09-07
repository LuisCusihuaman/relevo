import { useQuery } from "@tanstack/react-query";
import { api } from "../client";
import type { PaginatedHandovers, Handover } from "../types";
import { patientQueryKeys } from "./patients";

// Handover query keys (extending patient query keys)
export const handoverQueryKeys = {
	...patientQueryKeys,
	handovers: () => [...patientQueryKeys.all, "handovers"] as const,
	handoversWithParams: (parameters?: { page?: number; pageSize?: number }) =>
		[...handoverQueryKeys.handovers(), parameters] as const,
	handoverById: (id: string) => [...handoverQueryKeys.handovers(), id] as const,
};

/**
 * Get all handovers for the current user
 */
export async function getHandovers(parameters?: {
	page?: number;
	pageSize?: number;
}): Promise<PaginatedHandovers> {
	const { data } = await api.get<PaginatedHandovers>("/me/handovers", { params: parameters });
	return data;
}

/**
 * Get a specific handover by ID
 */
export async function getHandover(handoverId: string): Promise<Handover> {
	const { data } = await api.get<Handover>(`/handovers/${handoverId}`);
	return data;
}

/**
 * Hook to get all handovers for the current user
 */
export function useHandovers(parameters?: {
	page?: number;
	pageSize?: number;
}): ReturnType<typeof useQuery<PaginatedHandovers | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.handoversWithParams(parameters),
		queryFn: () => getHandovers(parameters),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: PaginatedHandovers | undefined) => data,
	});
}

/**
 * Hook to get a specific handover by ID
 */
export function useHandover(handoverId: string): ReturnType<typeof useQuery<Handover | undefined, Error>> {
	return useQuery({
		queryKey: handoverQueryKeys.handoverById(handoverId),
		queryFn: () => getHandover(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: Handover | undefined) => data,
	});
}
