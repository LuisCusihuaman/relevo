import { useQuery } from "@tanstack/react-query";
import { api } from "../client";
import type { Schemas } from "@/api/generated";
import type { PatientSummaryCard, PatientDetail, HandoverSummary } from "@/types/domain";
import { mapApiPatientSummaryCard, mapApiPatientDetail } from "@/api/mappers";
import { mapApiHandoverRecord } from "@/api/mappers";

// Response types from API (using generated types)
type PaginatedPatientSummaryCards = {
	items: Array<PatientSummaryCard>;
	pagination: Schemas["PaginationInfo"];
};

type PaginatedHandovers = {
	items: Array<HandoverSummary>;
	pagination: Schemas["PaginationInfo"];
};

type PatientSummaryResponse = Schemas["GetPatientSummaryResponse"];
type CreatePatientSummaryRequest = Schemas["CreatePatientSummaryRequest"];
type UpdatePatientSummaryRequest = Schemas["UpdatePatientSummaryRequest"];
type PatientSummaryUpdateResponse = Schemas["UpdatePatientSummaryResponse"];

// Query Keys for cache invalidation
export const patientQueryKeys = {
	all: ["patients"] as const,
	allPatients: () => [...patientQueryKeys.all, "all"] as const,
	allPatientsWithParams: (parameters?: { page?: number; pageSize?: number }) =>
		[...patientQueryKeys.allPatients(), parameters] as const,
	assigned: () => [...patientQueryKeys.all, "assigned"] as const,
	assignedWithParams: (parameters?: { page?: number; pageSize?: number }) =>
		[...patientQueryKeys.assigned(), parameters] as const,
	details: () => [...patientQueryKeys.all, "details"] as const,
	detailsById: (id: string) => [...patientQueryKeys.details(), id] as const,
	handoverTimeline: () => [...patientQueryKeys.all, "handoverTimeline"] as const,
	handoverTimelineById: (
		id: string,
		parameters?: { page?: number; pageSize?: number }
	) => [...patientQueryKeys.handoverTimeline(), id, parameters] as const,
	summary: () => [...patientQueryKeys.all, "summary"] as const,
	summaryById: (id: string) => [...patientQueryKeys.summary(), id] as const,
};

/**
 * Get all patients across all units
 */
export async function getAllPatients(
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<PaginatedPatientSummaryCards> {
	const { data } = await api.get<Schemas["GetAllPatientsResponse"]>("/patients", { params: parameters });
	return {
		items: (data.items ?? []).map(mapApiPatientSummaryCard),
		pagination: data.pagination ?? { totalItems: 0, page: 1, pageSize: 10, totalPages: 0 },
	};
}

/**
 * Get assigned patients for the current user
 */
export async function getAssignedPatients(
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<PaginatedPatientSummaryCards> {
	const { data } = await api.get<Schemas["GetMyPatientsResponse"]>("/me/patients", { params: parameters });
	return {
		items: (data.items ?? []).map(mapApiPatientSummaryCard),
		pagination: data.pagination ?? { totalCount: 0, page: 1, pageSize: 10 },
	};
}

/**
 * Get detailed information for a specific patient
 */
export async function getPatientDetails(
	patientId: string
): Promise<PatientDetail> {
	const { data } = await api.get<Schemas["GetPatientByIdResponse"]>(`/patients/${patientId}`);
	return mapApiPatientDetail(data);
}

/**
 * Get handover timeline for a specific patient
 */
export async function getPatientHandoverTimeline(
	patientId: string,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<PaginatedHandovers> {
	const { data } = await api.get<Schemas["GetPatientHandoversResponse"]>(`/patients/${patientId}/handovers`, { params: parameters });
	return {
		items: (data.items ?? []).map(mapApiHandoverRecord),
		pagination: data.pagination ?? { totalItems: 0, page: 1, pageSize: 10, totalPages: 0 },
	};
}

/**
 * Get patient summary for a specific patient
 */
export async function getPatientSummary(
	patientId: string
): Promise<PatientSummaryResponse> {
	const { data } = await api.get<PatientSummaryResponse>(`/patients/${patientId}/summary`);
	return data;
}

/**
 * Create patient summary for a specific patient
 */
export async function createPatientSummary(
	patientId: string,
	request: CreatePatientSummaryRequest
): Promise<PatientSummaryResponse> {
	const { data } = await api.post<PatientSummaryResponse>(`/patients/${patientId}/summary`, request);
	return data;
}

/**
 * Update patient summary for a specific patient
 */
export async function updatePatientSummary(
	patientId: string,
	request: UpdatePatientSummaryRequest
): Promise<PatientSummaryUpdateResponse> {
	const { data } = await api.put<PatientSummaryUpdateResponse>(`/patients/${patientId}/summary`, request);
	return data;
}

/**
 * Hook to get all patients across all units
 */
export function useAllPatients(parameters?: {
	page?: number;
	pageSize?: number;
}): ReturnType<typeof useQuery<PaginatedPatientSummaryCards | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.allPatientsWithParams(parameters),
		queryFn: () => getAllPatients(parameters),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: PaginatedPatientSummaryCards | undefined) => data,
	});
}

/**
 * Hook to get assigned patients for the current user
 */
export function useAssignedPatients(parameters?: {
	page?: number;
	pageSize?: number;
}): ReturnType<typeof useQuery<PaginatedPatientSummaryCards | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.assignedWithParams(parameters),
		queryFn: () => getAssignedPatients(parameters),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: PaginatedPatientSummaryCards | undefined) => data,
	});
}

/**
 * Hook to get detailed information for a specific patient
 */
export function usePatientDetails(patientId: string): ReturnType<typeof useQuery<PatientDetail | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.detailsById(patientId),
		queryFn: () => getPatientDetails(patientId),
		enabled: !!patientId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: PatientDetail | undefined) => data,
	});
}

/**
 * Query options for patient handover timeline (for use with queryClient.fetchQuery)
 */
export function patientHandoverTimelineQueryOptions(
	patientId: string,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): {
	queryKey: ReturnType<typeof patientQueryKeys.handoverTimelineById>;
	queryFn: () => Promise<PaginatedHandovers>;
	staleTime: number;
	gcTime: number;
} {
	return {
		queryKey: patientQueryKeys.handoverTimelineById(patientId, parameters),
		queryFn: () => getPatientHandoverTimeline(patientId, parameters),
		staleTime: 2 * 60 * 1000, // 2 minutes
		gcTime: 5 * 60 * 1000, // 5 minutes
	};
}

/**
 * Hook to get handover timeline for a specific patient
 */
export function usePatientHandoverTimeline(
	patientId: string,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): ReturnType<typeof useQuery<PaginatedHandovers | undefined, Error>> {
	return useQuery({
		...patientHandoverTimelineQueryOptions(patientId, parameters),
		enabled: !!patientId,
		select: (data: PaginatedHandovers | undefined) => data,
	});
}

/**
 * Hook to get patient summary for a specific patient
 */
export function usePatientSummary(patientId: string): ReturnType<typeof useQuery<PatientSummaryResponse | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.summaryById(patientId),
		queryFn: () => getPatientSummary(patientId),
		enabled: !!patientId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: PatientSummaryResponse | undefined) => data,
	});
}
