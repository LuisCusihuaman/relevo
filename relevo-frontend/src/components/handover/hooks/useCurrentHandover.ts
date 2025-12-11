import { useMemo } from "react";
import { useParams, useRouterState } from "@tanstack/react-router";
import { useHandover } from "@/api/endpoints/handovers";
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import { formatPhysician, getInitials } from "@/lib/formatters";
import type { PatientHandoverData, HandoverDetail as Handover } from "@/types/domain";
import {
	useCurrentPhysician,
	type UserInfo,
} from "@/hooks/useCurrentPhysician";
import { usePatientCurrentHandover } from "@/hooks/usePatientCurrentHandover";

export interface CurrentHandoverData {
	handoverId: string;
	patientId: string;
	handoverData: Handover | null;
	patientData: PatientHandoverData | null;
	currentUser: UserInfo;
	assignedPhysician: { id?: string; name: string; initials: string; role: string };
	receivingPhysician: { id?: string; name: string; initials: string; role: string };
	isLoading: boolean;
	error: Error | null;
}

export function useCurrentHandover(): CurrentHandoverData {
	// Get current route to determine how to extract params
	const routerState = useRouterState();
	const currentPath = routerState.location.pathname;

	// Try to get params from new route structure
	const params = useParams({ strict: false }) as { 
		patientId?: string; 
		handoverId?: string;
	};

	// Resolve handoverId based on route
	// If we're on /patient/$patientId (no handoverId in URL), resolve from timeline
	// If we're on /patient/$patientId/history/$handoverId, use the handoverId from params
	const isHistoryRoute = currentPath.includes("/history/");
	const isPatientRoute = currentPath.startsWith("/patient/") && !isHistoryRoute;

	// Get patientId from params
	const patientId = params.patientId || "";

	// For patient route, get active handover from timeline
	const { 
		currentHandover: resolvedHandover,
		isLoading: resolvingHandover 
	} = usePatientCurrentHandover(isPatientRoute ? patientId : "");

	// Determine the handoverId to use
	const handoverId = isHistoryRoute 
		? (params.handoverId || "") 
		: isPatientRoute 
			? (resolvedHandover?.id || "") 
			: (params.handoverId || "");

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
			// Fallback to patientData name if responsiblePhysicianName is empty
			const name = handoverData.responsiblePhysicianName || patientData?.assignedPhysician?.name || "";
			return {
				id: handoverData.responsiblePhysicianId,
				name,
				initials: getInitials(name),
				role: patientData?.assignedPhysician?.role || "Doctor",
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
			patientId,
			handoverData: handoverData || null,
			patientData: patientData || null,
			currentUser,
			assignedPhysician,
			receivingPhysician,
			isLoading: resolvingHandover || handoverLoading || patientLoading,
			error: (handoverError as Error) || (patientError as Error) || null,
		}),
		[
			handoverId,
			patientId,
			handoverData,
			patientData,
			currentUser,
			assignedPhysician,
			receivingPhysician,
			resolvingHandover,
			handoverLoading,
			patientLoading,
			handoverError,
			patientError,
		],
	);
}
