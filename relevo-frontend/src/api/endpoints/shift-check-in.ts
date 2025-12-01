import { useQuery, useMutation } from "@tanstack/react-query";
import { type createAuthenticatedApiCall } from "../client";
import { useAuthenticatedApi } from "@/hooks/useAuthenticatedApi";
import type { Unit, Shift, AssignPatientsPayload, ShiftCheckInPatientsResponse } from "../types";
import type { ShiftCheckInPatient } from "@/common/types";
import { patientQueryKeys } from "./patients";

// Shift Check-In query keys
export const shiftCheckInQueryKeys = {
	...patientQueryKeys,
	shiftCheckIn: () => [...patientQueryKeys.all, "shift-check-in"] as const,
	units: () => [...shiftCheckInQueryKeys.shiftCheckIn(), "units"] as const,
	shifts: () => [...shiftCheckInQueryKeys.shiftCheckIn(), "shifts"] as const,
	patientsByUnit: (unitId: string) => [...shiftCheckInQueryKeys.shiftCheckIn(), "units", unitId, "patients"] as const,
};

/**
 * Get hospital units
 */
export async function getUnits(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>
): Promise<Array<Unit>> {
	const data = await authenticatedApiCall<{ units?: Array<Unit>; Units?: Array<Unit> }>({
		method: "GET",
		url: "/shift-check-in/units",
	});
	return data.units ?? data.Units ?? [];
}

/**
 * Get available shifts
 */
export async function getShifts(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>
): Promise<Array<Shift>> {
	const data = await authenticatedApiCall<{ shifts?: Array<Shift>; Shifts?: Array<Shift> }>({
		method: "GET",
		url: "/shift-check-in/shifts",
	});
	return data.shifts ?? data.Shifts ?? [];
}

/**
 * Get patients available for assignment by unit
 */
export async function getPatientsByUnit(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	unitId: string,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<Array<ShiftCheckInPatient>> {
	const data = await authenticatedApiCall<ShiftCheckInPatientsResponse>({
		method: "GET",
		url: `/units/${unitId}/patients`,
		params: parameters,
	});
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
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: shiftCheckInQueryKeys.units(),
		queryFn: () => getUnits(authenticatedApiCall),
		staleTime: 10 * 60 * 1000, // 10 minutes
		gcTime: 30 * 60 * 1000, // 30 minutes
		select: (data: Array<Unit> | undefined) => data,
	});
}

/**
 * Hook to get available shifts
 */
export function useShifts(): ReturnType<typeof useQuery<Array<Shift> | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: shiftCheckInQueryKeys.shifts(),
		queryFn: () => getShifts(authenticatedApiCall),
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
): ReturnType<typeof useQuery<Array<ShiftCheckInPatient> | undefined, Error>> {
	const { authenticatedApiCall } = useAuthenticatedApi();
	return useQuery({
		queryKey: shiftCheckInQueryKeys.patientsByUnit(unitId ?? ""),
		queryFn: () => getPatientsByUnit(authenticatedApiCall, unitId ?? "", parameters),
		enabled: parameters?.enabled ?? Boolean(unitId), // Use provided enabled or default to Boolean(unitId)
		staleTime: 5 * 60 * 1000, // 5 minutes
		gcTime: 15 * 60 * 1000, // 15 minutes
		select: (data: Array<ShiftCheckInPatient> | undefined) => data,
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
