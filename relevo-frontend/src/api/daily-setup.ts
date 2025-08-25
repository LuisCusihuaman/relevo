import {
	useMutation,
	useQuery,
	type UseMutationResult,
	type UseQueryResult,
} from "@tanstack/react-query";

const API_BASE_URL: string =
	import.meta.env["VITE_API_BASE_URL"] || "https://localhost:57679";

// Query Keys – Concise-FP naming
export const dailySetupKeys = {
	units: ["setup", "units"] as const,
	shifts: ["setup", "shifts"] as const,
	patientsByUnit: (unitId: string) => ["units", unitId, "patients"] as const,
};

// API Types (Strict-TS)
export type ApiUnit = { id: string; name: string; description?: string };
export type ApiShift = {
	id: string;
	name: string;
	startTime?: string;
	endTime?: string;
};
export type ApiPatient = {
	id: number | string;
	name: string;
	age?: number;
	room?: string;
	diagnosis?: string;
	status?: string;
	severity?: string;
};

type UnitsResponse = { units?: Array<ApiUnit>; Units?: Array<ApiUnit> };
type ShiftsResponse = { shifts?: Array<ApiShift>; Shifts?: Array<ApiShift> };
type PatientsResponse = { patients?: Array<ApiPatient>; Patients?: Array<ApiPatient> };

// Fetch helpers
async function getJson<T>(path: string): Promise<T> {
	const response = await fetch(new URL(path, API_BASE_URL).toString(), {
		headers: { Accept: "application/json" },
	});
	if (!response.ok) throw new Error(`Request failed: ${response.status}`);
	return (await response.json()) as T;
}

export function useUnitsQuery(): UseQueryResult<Array<ApiUnit>, Error> {
	return useQuery({
		queryKey: dailySetupKeys.units,
		queryFn: async () => {
			const data = await getJson<UnitsResponse>("/setup/units");
			return data.units ?? data.Units ?? [];
		},
	});
}

export function useShiftsQuery(): UseQueryResult<Array<ApiShift>, Error> {
	return useQuery({
		queryKey: dailySetupKeys.shifts,
		queryFn: async () => {
			const data = await getJson<ShiftsResponse>("/setup/shifts");
			return data.shifts ?? data.Shifts ?? [];
		},
	});
}

export function usePatientsByUnitQuery(
	unitId: string | undefined
): UseQueryResult<Array<ApiPatient>, Error> {
	return useQuery({
		queryKey: unitId
			? dailySetupKeys.patientsByUnit(unitId)
			: ["units", "", "patients"],
		queryFn: async () => {
			const data = await getJson<PatientsResponse>(
				`/units/${unitId ?? ""}/patients?page=1&pageSize=50`
			);
			return data.patients ?? data.Patients ?? [];
		},
		enabled: Boolean(unitId),
	});
}

export type AssignPayload = { shiftId: string; patientIds: Array<string> };

async function postAssign(payload: AssignPayload): Promise<void> {
	const response = await fetch(
		new URL("/me/assignments", API_BASE_URL).toString(),
		{
			method: "POST",
			headers: { "Content-Type": "application/json", Accept: "application/json" },
			body: JSON.stringify(payload),
		}
	);
	if (!response.ok) throw new Error(`Assign failed: ${response.status}`);
}

export function useAssignPatientsMutation(): UseMutationResult<
	void,
	Error,
	AssignPayload
> {
	return useMutation({ mutationFn: postAssign });
}
