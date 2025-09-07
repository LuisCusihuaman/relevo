import type { ReactElement } from "react";
import { useNavigate } from "@tanstack/react-router";
import {
	EntityListMobile,
	EntityTable,
	FilterToolbar,
	ListHeader,
} from "@/components/home";
import { handovers } from "@/pages/data";
import type { Handover } from "@/components/home/types";

export function Patients(): ReactElement {
	const navigate = useNavigate();

	const handleHandoverClick = (
		handoverId: string,
		patientName: string,
	): void => {
		// Find the handover by ID to get the patient information
		const handover: Handover | undefined = handovers.find((h: Handover): boolean => h.id === handoverId);

		if (handover) {
			const patientSlug: string = handover.patientKey || patientName.toLowerCase().replace(/[^a-z0-9]/g, "-");
			void navigate({ to: `/${patientSlug}/${handoverId}` });
		} else {
			// Fallback to patient name if handover not found
			const patientSlug: string = patientName.toLowerCase().replace(/[^a-z0-9]/g, "-");
			void navigate({ to: `/${patientSlug}/${handoverId}` });
		}
	};

	return (
		<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
			<ListHeader />
			<FilterToolbar />
			<EntityTable handleHandoverClick={handleHandoverClick} />
			<EntityListMobile handleHandoverClick={handleHandoverClick} />
		</div>
	);
}
