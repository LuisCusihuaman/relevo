import { useMemo } from "react";
import { useParams } from "@tanstack/react-router";
import { useHandover } from "@/api/endpoints/handovers";
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import { formatPhysician, getInitials } from "@/lib/formatters";
import type { PatientHandoverData, HandoverDetail as Handover } from "@/types/domain";
import {
	useCurrentPhysician,
	type UserInfo,
} from "@/hooks/useCurrentPhysician";

interface CurrentHandoverData {
	handoverId: string;
	handoverData: Handover | null;
	patientData: PatientHandoverData | null;
	currentUser: UserInfo;
	assignedPhysician: { name: string; initials: string; role: string };
	receivingPhysician: { name: string; initials: string; role: string };
	isLoading: boolean;
	error: Error | null;
}

export function useCurrentHandover(): CurrentHandoverData {
	// 1. Get ID from params
	const { handoverId } = useParams({
		from: "/_authenticated/$patientSlug/$handoverId",
	}) as unknown as { handoverId: string };

	// 2. Fetch Data (React Query handles deduping/caching)
	const {
		data: handoverData,
		isLoading: handoverLoading,
		error: handoverError,
	} = useHandover(handoverId);
	const {
		patientData,
		isLoading: patientLoading,
		error: patientError,
	} = usePatientHandoverData(handoverId);

	// 3. Get current user info from hook
	const currentUser = useCurrentPhysician();

	const assignedPhysician = useMemo(() => {
		if (handoverData) {
			return {
				id: handoverData.responsiblePhysicianId,
				name: handoverData.responsiblePhysicianName,
				initials: getInitials(handoverData.responsiblePhysicianName),
				role: "Doctor",
			};
		}
		return formatPhysician(patientData?.assignedPhysician);
	}, [handoverData, patientData?.assignedPhysician]);

	const receivingPhysician = useMemo(
		() => formatPhysician(patientData?.receivingPhysician),
		[patientData?.receivingPhysician],
	);

	// 4. Return stable object
	return useMemo(
		(): CurrentHandoverData => ({
			handoverId,
			handoverData: handoverData || null,
			patientData: patientData || null,
			currentUser,
			assignedPhysician,
			receivingPhysician,
			isLoading: handoverLoading || patientLoading,
			error: (handoverError as Error) || (patientError as Error) || null,
		}),
		[
			handoverId,
			handoverData,
			patientData,
			currentUser,
			assignedPhysician,
			receivingPhysician,
			handoverLoading,
			patientLoading,
			handoverError,
			patientError,
		],
	);
}
