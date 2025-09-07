import axios from "axios";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { SetupPatient } from "@/common/types";

// Axios instance with default config
const api = axios.create({
	baseURL: (import.meta.env["VITE_API_URL"] as string | undefined) || "https://localhost:57679",
	headers: {
		"Content-Type": "application/json",
		Accept: "application/json",
	},
});

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
	const token = localStorage.getItem("authToken");
	if (token) {
		config.headers.Authorization = `Bearer ${token}`;
	}
	return config;
});

// API Response Types (matching the OpenAPI schema)
export type PatientSummaryCard = {
	id: string;
	name: string;
	handoverStatus: "NotStarted" | "InProgress" | "Completed";
	handoverId: string | null;
};

export type PatientDetail = {
	id: string;
	name: string;
	mrn: string;
	dob: string;
	gender: "Male" | "Female" | "Other" | "Unknown";
	admissionDate: string;
	currentUnit: string;
	roomNumber: string;
	diagnosis: string;
	allergies: Array<string>;
	medications: Array<string>;
	notes: string;
};

export type PatientHandoverTimelineItem = {
	handoverId: string;
	status: "InProgress" | "Completed";
	createdAt: string;
	completedAt: string | null;
	shiftName: string;
	illnessSeverity: "Stable" | "Watcher" | "Unstable";
};

export type PaginationInfo = {
	totalItems: number;
	totalPages: number;
	currentPage: number;
	pageSize: number;
};

export type PaginatedPatientSummaryCards = {
	pagination: PaginationInfo;
	items: Array<PatientSummaryCard>;
};

export type PaginatedPatientHandoverTimeline = {
	pagination: PaginationInfo;
	items: Array<PatientHandoverTimelineItem>;
};

// API Functions

// Additional types for Daily Setup
export type Unit = {
	id: string;
	name: string;
	description?: string;
};

export type Shift = {
	id: string;
	name: string;
	startTime?: string;
	endTime?: string;
};


export type AssignPatientsPayload = {
	shiftId: string;
	patientIds: Array<string>;
};

type UnitsResponse = {
	units?: Array<Unit>;
	Units?: Array<Unit>;
};

type ShiftsResponse = {
	shifts?: Array<Shift>;
	Shifts?: Array<Shift>;
};

type SetupPatientsResponse = {
	patients?: Array<SetupPatient>;
	Patients?: Array<SetupPatient>;
};

/**
 * Get assigned patients for the current user
 */
export async function getAssignedPatients(parameters?: {
	page?: number;
	pageSize?: number;
}): Promise<PaginatedPatientSummaryCards> {
	const { data } = await api.get<PaginatedPatientSummaryCards>("/me/patients", { params: parameters });
	return data;
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
): Promise<PaginatedPatientHandoverTimeline> {
	const { data } = await api.get<PaginatedPatientHandoverTimeline>(`/patients/${patientId}/handovers`, { params: parameters });
	return data;
}

/**
 * Get hospital units
 */
export async function getUnits(): Promise<Array<Unit>> {
	const { data } = await api.get<UnitsResponse>("/setup/units");
	return data.units ?? data.Units ?? [];
}

/**
 * Get available shifts
 */
export async function getShifts(): Promise<Array<Shift>> {
	const { data } = await api.get<ShiftsResponse>("/setup/shifts");
	return data.shifts ?? data.Shifts ?? [];
}

/**
 * Get patients available for assignment by unit
 */
export async function getPatientsByUnit(unitId: string, parameters?: {
	page?: number;
	pageSize?: number;
}): Promise<Array<SetupPatient>> {
	const { data } = await api.get<SetupPatientsResponse>(`/units/${unitId}/patients`, { params: parameters });
	return data.patients ?? data.Patients ?? [];
}

/**
 * Assign patients to a shift
 */
export async function assignPatients(payload: AssignPatientsPayload): Promise<void> {
	await api.post("/me/assignments", payload);
}

// Query Keys for cache invalidation
export const patientQueryKeys = {
	all: ["patients"] as const,
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
	// Daily Setup keys
	setup: () => [...patientQueryKeys.all, "setup"] as const,
	units: () => [...patientQueryKeys.setup(), "units"] as const,
	shifts: () => [...patientQueryKeys.setup(), "shifts"] as const,
	patientsByUnit: (unitId: string) => [...patientQueryKeys.setup(), "units", unitId, "patients"] as const,
};

// React Query Hooks

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
 * Hook to get handover timeline for a specific patient
 */
export function usePatientHandoverTimeline(
	patientId: string,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): ReturnType<typeof useQuery<PaginatedPatientHandoverTimeline | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.handoverTimelineById(patientId, parameters),
		queryFn: () => getPatientHandoverTimeline(patientId, parameters),
		enabled: !!patientId,
		staleTime: 2 * 60 * 1000, // 2 minutes
		gcTime: 5 * 60 * 1000, // 5 minutes
		select: (data: PaginatedPatientHandoverTimeline | undefined) => data,
	});
}

// Mutation Hooks for future patient manipulation

/**
 * Hook to refresh assigned patients data
 */
export function useRefreshAssignedPatients(): ReturnType<typeof useMutation<PaginatedPatientSummaryCards, unknown, void, unknown>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: () => getAssignedPatients(),
		onSuccess: () => {
			// Invalidate and refetch assigned patients
			void queryClient.invalidateQueries({ queryKey: patientQueryKeys.assigned() });
		},
	});
}

/**
 * Hook to refresh patient details
 */
export function useRefreshPatientDetails(): ReturnType<typeof useMutation<PatientDetail, unknown, string, unknown>> {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: (patientId: string) => getPatientDetails(patientId),
		onSuccess: (_, patientId) => {
			// Invalidate specific patient details
			void queryClient.invalidateQueries({ queryKey: patientQueryKeys.detailsById(patientId) });
		},
	});
}

// Daily Setup Hooks

/**
 * Hook to get hospital units
 */
export function useUnits(): ReturnType<typeof useQuery<Array<Unit> | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.units(),
		queryFn: () => getUnits(),
		staleTime: 10 * 60 * 1000, // 10 minutes
		gcTime: 30 * 60 * 1000, // 30 minutes
		select: (data: Array<Unit> | undefined) => data,
	});
}

/**
 * Hook to get available shifts
 */
export function useShifts(): ReturnType<typeof useQuery<Array<Shift> | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.shifts(),
		queryFn: () => getShifts(),
		staleTime: 10 * 60 * 1000, // 10 minutes
		gcTime: 30 * 60 * 1000, // 30 minutes
		select: (data: Array<Shift> | undefined) => data,
	});
}

/**
 * Hook to get patients available for assignment by unit
 */
export function usePatientsByUnit(
	unitId: string | undefined,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): ReturnType<typeof useQuery<Array<SetupPatient> | undefined, Error>> {
	return useQuery({
		queryKey: patientQueryKeys.patientsByUnit(unitId ?? ""),
		queryFn: () => getPatientsByUnit(unitId ?? "", parameters),
		enabled: Boolean(unitId),
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 15 * 60 * 1000, // 15 minutes
		select: (data: Array<SetupPatient> | undefined) => data,
	});
}

/**
 * Hook to assign patients to a shift
 */
export function useAssignPatients(): ReturnType<typeof useMutation<void, Error, AssignPatientsPayload>> {
	return useMutation({
		mutationFn: assignPatients,
	});
}
