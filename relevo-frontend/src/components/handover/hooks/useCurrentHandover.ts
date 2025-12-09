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
	assignedPhysician: { id?: string; name: string; initials: string; role: string };
	receivingPhysician: { id?: string; name: string; initials: string; role: string };
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

	const receivingPhysician = useMemo(() => {
		// Priority 1: Use receiver from handover data (SSOT for the transaction)
		if (handoverData?.receiverUserId) {
			// If we had a way to get user details by ID sync, we'd use it.
			// For now, we might need to rely on the patient data fallback if name is missing,
			// or assume the UI will fetch user details elsewhere.
			// But wait, the handover object usually doesn't carry receiver name if it's just an ID.
			// However, looking at HandoverDetail type, it has `receiverUserId` but no `receiverName`.
			// We can try to match with patientData.receivingPhysician if IDs match, or use it as fallback.
		}
		
		// Fallback: Use patient data (which might be the planned receiver)
		const patientReceiver = formatPhysician(patientData?.receivingPhysician);
		
		// If we have handover data with receiver ID, try to enrich the patient receiver object
		if (handoverData?.receiverUserId && patientReceiver) {
			return {
				...patientReceiver,
				id: handoverData.receiverUserId // Ensure ID is from handover if available
			};
		}

		return patientReceiver;
	}, [handoverData?.receiverUserId, patientData?.receivingPhysician]);

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
