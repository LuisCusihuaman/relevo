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

export type HomeProps = {
	projectSlug?: string;
	initialTab?: string;
};

export function Home({
	projectSlug,
	initialTab = "Resumen",
}: HomeProps): ReactElement {
	const [searchQuery, setSearchQuery] = useState<string>("");
	const [isSearchOpen, setIsSearchOpen] = useState<boolean>(false);
	const [isMobileMenuOpen, setIsMobileMenuOpen] = useState<boolean>(false);
	const [activeTab, setActiveTab] = useState<string>(initialTab);

	useEffect(() => {
		setActiveTab(initialTab);
	}, [initialTab]);

	const currentProject =
		(projectSlug && patients.find((p) => p.name === projectSlug)) || null;
	const isProjectView = Boolean(currentProject);

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
		projectName: string
	): void => {
		const projectSlugMap: { [key: string]: string } = {
			"calendar-app": "calendar-app",
			"heroes-app": "heroes-app",
			"relevo-app": "relevo-app",
			"eduardoc/spanish": "eduardoc-spanish",
		};

		const projectSlug =
			projectSlugMap[projectName] ||
			projectName.toLowerCase().replace(/[^a-z0-9]/g, "-");
		window.location.href = `/${projectSlug}/${handoverId}`;
	};

	return (
		<div className="min-h-screen bg-gray-50">
			<AppHeader
				currentProject={currentProject}
				isMobileMenuOpen={isMobileMenuOpen}
				isProjectView={isProjectView}
				setIsMobileMenuOpen={setIsMobileMenuOpen}
				setIsSearchOpen={setIsSearchOpen}
			/>

			{isMobileMenuOpen && (
				<MobileMenu
					currentProject={currentProject}
					isProjectView={isProjectView}
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

			<SubNavigation
				activeTab={activeTab}
				setActiveTab={setActiveTab}
			/>

			<div className="flex-1 p-6">
				{!isProjectView && activeTab === "Resumen" && (
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

				{!isProjectView && activeTab === "Traspasos" && (
					<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
						<ListHeader />
						<FilterToolbar />
						<EntityTable handleHandoverClick={handleHandoverClick} />
						<EntityListMobile
							handleHandoverClick={handleHandoverClick}
						/>
					</div>
				)}

				{isProjectView && currentProject ? (
					<div className="space-y-6">
						{activeTab === "Resumen" && (
							<PatientProfileHeader currentProject={currentProject} />
						)}
					</div>
				) : null}
			</div>
		</div>
	);
}
