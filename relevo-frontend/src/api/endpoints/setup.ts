import { useQuery, useMutation } from "@tanstack/react-query";
import { api, type createAuthenticatedApiCall } from "../client";
import { useAuthenticatedApi } from "@/hooks/useAuthenticatedApi";
import type { Unit, Shift, AssignPatientsPayload, SetupPatientsResponse } from "../types";
import type { SetupPatient } from "@/common/types";
import { patientQueryKeys } from "./patients";

// Setup query keys
export const setupQueryKeys = {
	...patientQueryKeys,
	setup: () => [...patientQueryKeys.all, "setup"] as const,
	units: () => [...setupQueryKeys.setup(), "units"] as const,
	shifts: () => [...setupQueryKeys.setup(), "shifts"] as const,
	patientsByUnit: (unitId: string) => [...setupQueryKeys.setup(), "units", unitId, "patients"] as const,
};

/**
 * Get hospital units
 */
export async function getUnits(): Promise<Array<Unit>> {
	const { data } = await api.get<{ units?: Array<Unit>; Units?: Array<Unit> }>("/setup/units");
	return data.units ?? data.Units ?? [];
}

/**
 * Get available shifts
 */
export async function getShifts(): Promise<Array<Shift>> {
	const { data } = await api.get<{ shifts?: Array<Shift>; Shifts?: Array<Shift> }>("/setup/shifts");
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
export async function assignPatients(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	payload: AssignPatientsPayload
): Promise<void> {
	await authenticatedApiCall({
		method: "POST",
		url: "/me/assignments",
		data: payload,
	});
}

/**
 * Hook to get hospital units
 */
export function useUnits(): ReturnType<typeof useQuery<Array<Unit> | undefined, Error>> {
	return useQuery({
		queryKey: setupQueryKeys.units(),
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
		queryKey: setupQueryKeys.shifts(),
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
		enabled?: boolean;
	}
): ReturnType<typeof useQuery<Array<SetupPatient> | undefined, Error>> {
	return useQuery({
		queryKey: setupQueryKeys.patientsByUnit(unitId ?? ""),
		queryFn: () => getPatientsByUnit(unitId ?? "", parameters),
		enabled: parameters?.enabled ?? Boolean(unitId), // Use provided enabled or default to Boolean(unitId)
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 15 * 60 * 1000, // 15 minutes
		select: (data: Array<SetupPatient> | undefined) => data,
	});
}

/**
 * Hook to assign patients to a shift
 */
export function useAssignPatients(): ReturnType<typeof useMutation<void, Error, AssignPatientsPayload>> {
	const { authenticatedApiCall } = useAuthenticatedApi();

	return useMutation({
		mutationFn: (payload: AssignPatientsPayload) => assignPatients(authenticatedApiCall, payload),
	});
}
