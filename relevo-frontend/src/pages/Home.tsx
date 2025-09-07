import { type ReactElement, useEffect, useState } from "react";
import {
	AppHeader,
	CommandPalette,
	DashboardSidebar,
	EntityListMobile,
	EntityTable,
	FilterToolbar,
	ListHeader,
	MobileMenu,
	PatientDirectoryList,
	PatientDirectoryToolbar,
	PatientProfileHeader,
	SubNavigation,
	VersionNotice,
} from "@/components/home";
import {
	patients,
	recentPreviews,
	searchResults,
} from "@/pages/data";
import type { Patient } from "@/components/home/types";

export type HomeProps = {
	patientSlug?: string;
	initialTab?: string;
};

export function Home({
	patientSlug,
	initialTab = "Resumen",
}: HomeProps): ReactElement {
	const [searchQuery, setSearchQuery] = useState<string>("");
	const [isSearchOpen, setIsSearchOpen] = useState<boolean>(false);
	const [isMobileMenuOpen, setIsMobileMenuOpen] = useState<boolean>(false);
	const [activeTab, setActiveTab] = useState<string>(initialTab);

	useEffect(() => {
		setActiveTab(initialTab === "Traspasos" ? "Pacientes" : initialTab);
	}, [initialTab]);

	const patientsList: ReadonlyArray<Patient> = patients as ReadonlyArray<Patient>;
	const currentPatient: Patient | null =
		patientSlug ? (patientsList.find((p: Patient): boolean => p.name === patientSlug) ?? null) : null;
	const isPatientView: boolean = Boolean(currentPatient);

	useEffect(() => {
		const handleKeyDown = (event_: KeyboardEvent): void => {
			if (event_.key === "f" || event_.key === "F") {
				event_.preventDefault();
				setIsSearchOpen(true);
			}
			if (event_.key === "Escape") {
				setIsSearchOpen(false);
			}
		};
		document.addEventListener("keydown", handleKeyDown);
		return (): void => {
			document.removeEventListener("keydown", handleKeyDown);
		};
	}, []);

	useEffect(() => {
		if (isMobileMenuOpen) {
			document.body.style.overflow = "hidden";
		} else {
			document.body.style.overflow = "unset";
		}
		return (): void => {
			document.body.style.overflow = "unset";
		};
	}, [isMobileMenuOpen]);

	const handleHandoverClick = (
		handoverId: string,
		patientName: string
	): void => {
		const patientSlugMap: { [key: string]: string } = {
			"calendar-app": "calendar-app",
			"heroes-app": "heroes-app",
			"relevo-app": "relevo-app",
			"eduardoc/spanish": "eduardoc-spanish",
		};

		const patientSlug =
			patientSlugMap[patientName] ||
			patientName.toLowerCase().replace(/[^a-z0-9]/g, "-");
		window.location.href = `/${patientSlug}/${handoverId}`;
	};

	return (
		<div className="min-h-screen bg-gray-50">
			<AppHeader
				currentPatient={currentPatient}
				isMobileMenuOpen={isMobileMenuOpen}
				isPatientView={isPatientView}
				setIsMobileMenuOpen={setIsMobileMenuOpen}
				setIsSearchOpen={setIsSearchOpen}
			/>

			{isMobileMenuOpen && (
				<MobileMenu
					currentPatient={currentPatient}
					isPatientView={isPatientView}
					setIsMobileMenuOpen={setIsMobileMenuOpen}
				/>
			)}

			{isSearchOpen && (
				<CommandPalette
					searchQuery={searchQuery}
					searchResults={searchResults}
					setIsSearchOpen={setIsSearchOpen}
					setSearchQuery={setSearchQuery}
				/>
			)}

			<SubNavigation activeTab={activeTab} setActiveTab={setActiveTab} />

			<div className="flex-1 p-6">
				{!isPatientView && activeTab === "Resumen" && (
					<div className="space-y-6">
						<VersionNotice />
						<div className="max-w-7xl mx-auto px-6 py-6">
							<PatientDirectoryToolbar />
							<div className="flex flex-col lg:flex-row gap-8">
								<DashboardSidebar recentPreviews={recentPreviews} />

								<PatientDirectoryList />
							</div>
						</div>
					</div>
				)}

				{!isPatientView && activeTab === "Pacientes" && (
					<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
						<ListHeader />
						<FilterToolbar />
						<EntityTable handleHandoverClick={handleHandoverClick} />
						<EntityListMobile handleHandoverClick={handleHandoverClick} />
					</div>
				)}

				{isPatientView && currentPatient ? (
					<div className="space-y-6">
						{activeTab === "Resumen" && (
							<PatientProfileHeader currentPatient={currentPatient} />
						)}
					</div>
				) : null}
			</div>
		</div>
	);
}
