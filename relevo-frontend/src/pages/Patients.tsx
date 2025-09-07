import type { ReactElement } from "react";
import {
	EntityListMobile,
	EntityTable,
	FilterToolbar,
	ListHeader,
} from "@/components/home";

export function Patients(): ReactElement {
	const handleHandoverClick = (
		handoverId: string,
		patientName: string,
	): void => {
		const patientSlugMap: { [key: string]: string } = {
			"calendar-app": "calendar-app",
			"heroes-app": "heroes-app",
			"relevo-app": "relevo-app",
			"eduardoc/spanish": "eduardoc-spanish",
		};

		const newPatientSlug =
			patientSlugMap[patientName] ||
			patientName.toLowerCase().replace(/[^a-z0-9]/g, "-");
		window.location.href = `/${newPatientSlug}/${handoverId}`;
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
