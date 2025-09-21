import { useQuery } from "@tanstack/react-query";
import { api, type createAuthenticatedApiCall } from "../client";
import { useAuthenticatedApi } from "@/hooks/useAuthenticatedApi";
import type {
	PaginatedPatientSummaryCards,
	PatientDetail,
	PaginatedHandovers,
	PatientSummaryResponse,
	CreatePatientSummaryRequest,
	UpdatePatientSummaryRequest,
	PatientSummaryUpdateResponse,
} from "../types";

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
export async function getAllPatients(parameters?: {
	page?: number;
	pageSize?: number;
}): Promise<PaginatedPatientSummaryCards> {
	const { data } = await api.get<PaginatedPatientSummaryCards>("/patients", { params: parameters });
	return data;
}

/**
 * Get assigned patients for the current user
 */
export async function getAssignedPatients(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<PaginatedPatientSummaryCards> {
	return authenticatedApiCall<PaginatedPatientSummaryCards>({
		method: "GET",
		url: "/me/patients",
		params: parameters,
	});
}

/**
 * Get detailed information for a specific patient
 */
export async function getPatientDetails(patientId: string): Promise<PatientDetail> {
	const { data } = await api.get<PatientDetail>(`/patients/${patientId}`);
	return data;
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
	const { data } = await api.get<PaginatedHandovers>(`/patients/${patientId}/handovers`, { params: parameters });
	return data;
}

/**
 * Get patient summary for a specific patient
 */
export async function getPatientSummary(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	patientId: string
): Promise<PatientSummaryResponse> {
	return authenticatedApiCall<PatientSummaryResponse>({
		method: "GET",
		url: `/patients/${patientId}/summary`,
	});
}

/**
 * Create patient summary for a specific patient
 */
export async function createPatientSummary(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	patientId: string,
	request: CreatePatientSummaryRequest
): Promise<PatientSummaryResponse> {
	return authenticatedApiCall<PatientSummaryResponse>({
		method: "POST",
		url: `/patients/${patientId}/summary`,
		data: request,
	});
}

/**
 * Update patient summary for a specific patient
 */
export async function updatePatientSummary(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	patientId: string,
	request: UpdatePatientSummaryRequest
): Promise<PatientSummaryUpdateResponse> {
	return authenticatedApiCall<PatientSummaryUpdateResponse>({
		method: "PUT",
		url: `/patients/${patientId}/summary`,
		data: request,
	});
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
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useQuery({
		queryKey: patientQueryKeys.assignedWithParams(parameters),
		queryFn: () => getAssignedPatients(authenticatedApiCall, parameters),
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
		queryKey: patientQueryKeys.handoverTimelineById(patientId, parameters),
		queryFn: () => getPatientHandoverTimeline(patientId, parameters),
		enabled: !!patientId,
		staleTime: 2 * 60 * 1000, // 2 minutes
		gcTime: 5 * 60 * 1000, // 5 minutes
		select: (data: PaginatedHandovers | undefined) => data,
	});
}

/**
 * Hook to get patient summary for a specific patient
 */
export function usePatientSummary(patientId: string): ReturnType<typeof useQuery<PatientSummaryResponse | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useQuery({
		queryKey: patientQueryKeys.summaryById(patientId),
		queryFn: () => getPatientSummary(authenticatedApiCall, patientId),
		enabled: !!patientId,
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 10 * 60 * 1000, // 10 minutes
		select: (data: PatientSummaryResponse | undefined) => data,
	});
}
