import { useQuery, useMutation } from "@tanstack/react-query";
import { api } from "../client";
import type { Unit, Shift, AssignPatientsPayload, ShiftCheckInPatientsResponse } from "../types";
import type { ShiftCheckInPatient } from "@/types/domain";

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
export async function getUnits(): Promise<Array<Unit>> {
	const { data } = await api.get<{ units?: Array<Unit>; Units?: Array<Unit> }>("/shift-check-in/units");
	return data.units ?? data.Units ?? [];
}

/**
 * Get available shifts
 */
export async function getShifts(): Promise<Array<Shift>> {
	const { data } = await api.get<{ shifts?: Array<Shift>; Shifts?: Array<Shift> }>("/shift-check-in/shifts");
	return data.shifts ?? data.Shifts ?? [];
}

/**
 * Get patients available for assignment by unit
 */
export async function getPatientsByUnit(
	unitId: string,
	parameters?: {
		page?: number;
		pageSize?: number;
	}
): Promise<Array<ShiftCheckInPatient>> {
	const { data } = await api.get<ShiftCheckInPatientsResponse>(`/units/${unitId}/patients`, { params: parameters });
	return data.patients ?? data.Patients ?? [];
}

/**
 * Assign patients to a shift
 */
export async function assignPatients(
	payload: AssignPatientsPayload
): Promise<void> {
	await api.post("/me/assignments", payload);
}

/**
 * Hook to get hospital units
 */
export function useUnits(): ReturnType<typeof useQuery<Array<Unit> | undefined, Error>> {
	return useQuery({
		queryKey: shiftCheckInQueryKeys.units(),
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
		queryKey: shiftCheckInQueryKeys.shifts(),
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
): ReturnType<typeof useQuery<Array<ShiftCheckInPatient> | undefined, Error>> {
	return useQuery({
		queryKey: shiftCheckInQueryKeys.patientsByUnit(unitId ?? ""),
		queryFn: () => getPatientsByUnit(unitId ?? "", parameters),
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
	return useMutation({
		mutationFn: (payload: AssignPatientsPayload) => assignPatients(payload),
	});
}
