import { usePatientHandoverData as usePatientHandoverDataApi } from "@/api/endpoints/handovers";
import type { PatientHandoverData } from "@/api";

export function usePatientHandoverData(handoverId?: string): {
	patientData: PatientHandoverData | null;
	isLoading: boolean;
	error: Error | null;
} {
	// Use the backend API that does all transformations
	const { data: patientData, isLoading, error } = usePatientHandoverDataApi(handoverId || "");

	return {
		patientData: patientData || null,
		isLoading,
		error,
	};
}
