import { useMemo, type ReactElement } from "react";
import { useNavigate } from "@tanstack/react-router";
import {
	EntityListMobile,
	EntityTable,
	FilterToolbar,
	ListHeader,
} from "@/components/home";
import type { HandoverUI as Handover } from "@/types/domain";
import { useAllPatients, mapApiPatientToUiHandover } from "@/api";

export function Patients(): ReactElement {
	const navigate = useNavigate();
	const { data: patientsData, isLoading, error } = useAllPatients();

	// Memoize the mapped patients to handovers to avoid unnecessary re-computations
	const handovers: ReadonlyArray<Handover> = useMemo(() => {
		if (!patientsData?.items) return [];
		return patientsData.items.map(mapApiPatientToUiHandover);
	}, [patientsData]);

	const handleHandoverClick = (
		handoverId: string,
		patientName: string,
	): void => {
		// Find the handover by ID to get the patient information
		const handover = handovers.find(h => h.id === handoverId);

		if (handover) {
			const patientSlug: string = handover?.patientKey || patientName.toLowerCase().replace(/[^a-z0-9]/g, "-");
			void navigate({ to: `/${patientSlug}/${handoverId}` });
		} else {
			// Fallback to patient name if handover not found
			const patientSlug: string = patientName.toLowerCase().replace(/[^a-z0-9]/g, "-");
			void navigate({ to: `/${patientSlug}/${handoverId}` });
		}
	};

	// Show loading state
	if (isLoading) {
		return (
			<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
				<ListHeader />
				<FilterToolbar />
				<EntityTable loading handleHandoverClick={() => {}} handovers={[]} />
				<div className="mt-6">
					<EntityListMobile loading handleHandoverClick={() => {}} handovers={[]} />
				</div>
			</div>
		);
	}

	// Show error state
	if (error) {
		return (
			<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
				<div className="flex items-center justify-center h-64">
					<div className="text-center">
						<div className="text-red-600 mb-2">Error al cargar traspasos</div>
						<p className="text-gray-600">Intente recargar la página</p>
					</div>
				</div>
			</div>
		);
	}

	return (
		<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
			<ListHeader />
			<FilterToolbar />
			<EntityTable handleHandoverClick={handleHandoverClick} handovers={handovers} />
			<EntityListMobile handleHandoverClick={handleHandoverClick} handovers={handovers} />
		</div>
	);
}
