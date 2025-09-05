import { type ReactElement, useEffect, useState } from "react";
import {
	DashboardSidebar,
	DeploymentsHeader,
	DeploymentsMobileList,
	DeploymentsTable,
	DeploymentsToolbar,
	Header,
	MobileMenu,
	NodeVersionWarning,
	ProjectHeader,
	ProjectsList,
	ProjectsToolbar,
	SearchOverlay,
	SubNavigation,
	type Project,
	type SearchResult,
} from "@/components/home";

const projects: Array<Project> = [
	{
		name: "relevo-app",
		url: "relevo-app.vercel.app",
		status: "Connect Git Repository",
		date: "Jul 12",
		icon: "react",
		hasGithub: false,
	},
	{
		name: "v0-portfolio-template-by-v0",
		url: "v0-portfolio-template-by-v0-mu-...",
		status: "Connect Git Repository",
		date: "Aug 17",
		icon: "react",
		hasGithub: false,
	},
	{
		name: "heroes-app",
		url: "No Production Deployment",
		status: "Create deploy.yml",
		date: "2/22/21",
		branch: "master",
		github: "LuisCusihuaman/Hero...",
		icon: "react",
		hasGithub: true,
	},
	{
		name: "calendar-app",
		url: "No Production Deployment",
		status: "docs: README.md",
		date: "8/8/20",
		branch: "master",
		github: "LuisCusihuaman/Cale...",
		icon: "react",
		hasGithub: true,
	},
	{
		name: "psa-frontend",
		url: "psa-frontend-alpha.vercel.app",
		status: "Connect Git Repository",
		date: "6/30/24",
		icon: "react",
		hasGithub: false,
	},
	{
		name: "image-component",
		url: "image-component-sandy-three.v...",
		status: "Initial commit",
		date: "2/5/21",
		branch: "master",
		github: "LuisCusihuaman/ima...",
		icon: "react",
		hasGithub: true,
	},
	{
		name: "v0-music-game-concept",
		url: "v0-music-game-concept.vercel.a...",
		status: "Connect Git Repository",
		date: "Mar 25",
		icon: "react",
		hasGithub: false,
	},
	{
		name: "backoffice",
		url: "backoffice-pi-dusky.vercel.app",
		status: "Initial commit",
		date: "10/16/23",
		branch: "main",
		github: "LuisCusihuaman/bac...",
		icon: "vercel",
		hasGithub: true,
	},
];

const recentPreviews = [
	{
		title: "Add new authentication flow",
		avatars: [
			{ src: null, fallback: "LC", bg: "bg-blue-500" },
			{ src: null, fallback: "JD", bg: "bg-green-500" },
		],
		status: "Source",
		pr: "#123",
		color: "Ready",
	},
	{
		title: "Update dashboard components",
		avatars: [{ src: null, fallback: "SM", bg: "bg-purple-500" }],
		status: "Error",
		pr: "#124",
	},
	{
		title: "Fix mobile responsive issues",
		avatars: [
			{ src: null, fallback: "AB", bg: "bg-orange-500" },
			{ src: null, fallback: "CD", bg: "bg-pink-500" },
		],
		status: "Source",
		pr: "#125",
		color: "Ready",
	},
];

export type HomeProps = {
	projectSlug?: string;
	initialTab?: string;
};

export function Home({
	projectSlug,
	initialTab = "Overview",
}: HomeProps): ReactElement {
	const [searchQuery, setSearchQuery] = useState<string>("");
	const [isSearchOpen, setIsSearchOpen] = useState<boolean>(false);
	const [isMobileMenuOpen, setIsMobileMenuOpen] = useState<boolean>(false);
	const [activeTab, setActiveTab] = useState<string>(initialTab);

	useEffect(() => {
		setActiveTab(initialTab);
	}, [initialTab]);

	const currentProject =
		(projectSlug && projects.find((p) => p.name === projectSlug)) || null;
	const isProjectView = Boolean(currentProject);
	const [searchResults] = useState<Array<SearchResult>>([
		{
			name: "eduardoc/spanish",
			category: "Latest Deployment",
			type: "deployment",
		},
		{ name: "main", category: "Latest Deployment", type: "deployment" },
		{ name: "Luis Cusihuaman's projects", category: "Team", type: "team" },
		{ name: "relevo-app", category: "Project", type: "project" },
		{ name: "Analytics", category: "Project", type: "project" },
		{
			name: '"visitors this week"',
			category: "Navigation Assistant",
			type: "assistant",
		},
	]);

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

	const handleDeploymentClick = (
		deploymentId: string,
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
		window.location.href = `/${projectSlug}/${deploymentId}`;
	};

	return (
		<div className="min-h-screen bg-gray-50">
			<Header
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
				<SearchOverlay
					searchQuery={searchQuery}
					searchResults={searchResults}
					setIsSearchOpen={setIsSearchOpen}
					setSearchQuery={setSearchQuery}
				/>
			)}

			<SubNavigation
				activeTab={activeTab}
				isProjectView={isProjectView}
				setActiveTab={setActiveTab}
			/>

			<div className="flex-1 p-6">
				{!isProjectView && activeTab === "Overview" && (
					<div className="space-y-6">
						<NodeVersionWarning />
						<div className="max-w-7xl mx-auto px-6 py-6">
							<ProjectsToolbar />
							<div className="flex flex-col lg:flex-row gap-8">
								<DashboardSidebar recentPreviews={recentPreviews} />
								<ProjectsList projects={projects} />
							</div>
						</div>
					</div>
				)}

				{!isProjectView && activeTab === "Deployments" && (
					<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
						<DeploymentsHeader />
						<DeploymentsToolbar />
						<DeploymentsTable handleDeploymentClick={handleDeploymentClick} />
						<DeploymentsMobileList
							handleDeploymentClick={handleDeploymentClick}
						/>
					</div>
				)}

				{isProjectView && currentProject ? (
					<div className="space-y-6">
						{activeTab === "Overview" && (
							<ProjectHeader currentProject={currentProject} />
						)}
					</div>
				) : null}
			</div>
		</div>
	);
}
