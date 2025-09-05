import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
	Popover,
	PopoverContent,
	PopoverTrigger,
} from "@/components/ui/popover";
import {
	Search,
	ChevronDown,
	GitBranch,
	AlertTriangle,
	Bell,
	Grid2X2,
	User,
	Settings,
	Plus,
	Command,
	Monitor,
	Sun,
	Moon,
	LogOut,
	RotateCcw,
	Calendar,
	Filter,
	Grid3X3,
	List,
	LucideChevronDown as DropdownMenuChevronDown,
	Github,
	Activity,
	MoreHorizontal,
	HomeIcon,
} from "lucide-react";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Badge } from "@/components/ui/badge";
import { type ReactElement, useEffect, useState } from "react";

const projects = [
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

	const currentProject = projectSlug
		? projects.find((p) => p.name === projectSlug)
		: null;
	const isProjectView = Boolean(currentProject);
	const [searchResults] = useState([
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
		// Map project display names to slugs
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
			{/* START: Header */}
			<header className="flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 md:px-6">
				<div className="flex items-center gap-3">
					{/* Vercel Logo */}
					<div className="flex items-center gap-3">
						<svg fill="currentColor" height="24" viewBox="0 0 75 65" width="24">
							<path d="M37.59.25l36.95 64H.64l36.95-64z" />
						</svg>

						<div className="flex items-center gap-2">
							{isProjectView ? (
								<div className="flex items-center gap-2">
									<Avatar className="h-7 w-7 md:hidden">
										<AvatarImage src="/placeholder.svg?height=28&width=28" />
										<AvatarFallback>LC</AvatarFallback>
									</Avatar>
									<Avatar className="h-7 w-7 hidden md:block">
										<AvatarImage src="/placeholder.svg?height=28&width=28" />
										<AvatarFallback>LC</AvatarFallback>
									</Avatar>
									<button
										className="text-base font-medium text-gray-900 hover:text-gray-700"
										onClick={() => (window.location.href = "/")}
									>
										Luis Cusihuaman's projects
									</button>
									<ChevronDown className="h-4 w-4 text-gray-400" />
									<div className="flex items-center gap-2">
										{currentProject?.name === "relevo-app" ? (
											<div className="w-5 h-5 bg-purple-600 rounded flex items-center justify-center">
												<span className="text-white text-xs font-bold">V</span>
											</div>
										) : (
											<div className="w-5 h-5 bg-gray-400 rounded-full"></div>
										)}
										<span className="text-base font-medium text-gray-900">
											{currentProject?.name}
										</span>
									</div>
									<ChevronDown className="h-4 w-4 text-gray-400" />
								</div>
							) : (
								<div className="flex items-center gap-2">
									<Avatar className="h-7 w-7">
										<AvatarImage src="/placeholder.svg?height=28&width=28" />
										<AvatarFallback>LC</AvatarFallback>
									</Avatar>
									<span className="text-base font-medium text-gray-900">
										Luis Cusihuaman's projects
									</span>
									<ChevronDown className="h-4 w-4 text-gray-400" />
								</div>
							)}
						</div>
					</div>
				</div>

				<div className="flex items-center gap-2 md:gap-4">
					{/* Search Field */}
					<div className="relative hidden md:block">
						<Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
						<Input
							readOnly
							className="pl-10 pr-8 w-48 h-8 text-sm border-gray-300 focus:border-gray-400 focus:ring-0 cursor-pointer"
							placeholder="Find..."
							onClick={() => {
								setIsSearchOpen(true);
							}}
						/>
						<kbd className="absolute right-2 top-1/2 transform -translate-y-1/2 text-xs text-gray-400 bg-gray-100 px-1 py-0.5 rounded">
							F
						</kbd>
					</div>

					{/* Feedback */}
					<Button
						className="text-sm text-gray-600 hover:text-gray-900 h-8 hidden md:flex"
						size="sm"
						variant="ghost"
					>
						Feedback
					</Button>

					<Button
						className={`h-8 w-8 p-0 text-gray-600 hover:text-gray-900 md:hidden ${isMobileMenuOpen ? "hidden" : ""}`}
						size="sm"
						variant="ghost"
						onClick={() => {
							setIsSearchOpen(true);
						}}
					>
						<Search className="h-4 w-4" />
					</Button>

					{/* Notification Bell */}
					<Popover>
						<PopoverTrigger asChild>
							<div className={`relative ${isMobileMenuOpen ? "hidden" : ""}`}>
								<Button
									className="h-8 w-8 p-0 text-gray-600 hover:text-gray-900"
									size="sm"
									variant="ghost"
								>
									<Bell className="h-4 w-4" />
								</Button>
								<div className="absolute -top-1 -right-1 h-2 w-2 bg-blue-500 rounded-full"></div>
							</div>
						</PopoverTrigger>
						<PopoverContent align="end" className="w-96 p-0 z-50">
							<div className="border-b border-gray-100">
								<div className="flex items-center justify-between px-4 py-3">
									<div className="flex items-center gap-6">
										<button className="text-sm font-medium text-gray-900 border-b-2 border-black pb-1">
											Inbox
										</button>
										<button className="text-sm text-gray-600 hover:text-gray-900 pb-1">
											Archive
										</button>
										<button className="text-sm text-gray-600 hover:text-gray-900 pb-1">
											Comments
										</button>
									</div>
									<Button
										className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600"
										size="sm"
										variant="ghost"
									>
										<Settings className="h-4 w-4" />
									</Button>
								</div>
							</div>

							<div className="max-h-96 overflow-y-auto">
								<div className="px-4 py-3 border-b border-gray-100">
									<div className="flex items-center gap-3">
										<div className="h-8 w-8 rounded-full bg-orange-100 flex items-center justify-center flex-shrink-0">
											<AlertTriangle className="h-4 w-4 text-orange-600" />
										</div>
										<div className="flex-1 min-w-0">
											<p className="text-sm text-gray-900 font-medium">
												<span className="font-semibold">calendar-app</span>{" "}
												failed to deploy in the{" "}
												<span className="font-semibold">Preview</span>{" "}
												environment
											</p>
											<p className="text-xs text-gray-500 mt-1">2d ago</p>
										</div>
										<Button
											className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600 flex-shrink-0"
											size="sm"
											variant="ghost"
										>
											<svg
												className="h-4 w-4"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"
													strokeLinecap="round"
													strokeLinejoin="round"
													strokeWidth={2}
												/>
											</svg>
										</Button>
									</div>
								</div>

								<div className="px-4 py-3 border-b border-gray-100">
									<div className="flex items-center gap-3">
										<div className="h-8 w-8 rounded-full bg-orange-100 flex items-center justify-center flex-shrink-0">
											<AlertTriangle className="h-4 w-4 text-orange-600" />
										</div>
										<div className="flex-1 min-w-0">
											<p className="text-sm text-gray-900 font-medium">
												<span className="font-semibold">calendar-app</span>{" "}
												failed to deploy in the{" "}
												<span className="font-semibold">Preview</span>{" "}
												environment
											</p>
											<p className="text-xs text-gray-500 mt-1">2d ago</p>
										</div>
										<Button
											className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600 flex-shrink-0"
											size="sm"
											variant="ghost"
										>
											<svg
												className="h-4 w-4"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"
													strokeLinecap="round"
													strokeLinejoin="round"
													strokeWidth={2}
												/>
											</svg>
										</Button>
									</div>
								</div>

								<div className="px-4 py-3 border-b border-gray-100">
									<div className="flex items-center gap-3">
										<Avatar className="h-8 w-8 flex-shrink-0">
											<AvatarImage src="/placeholder.svg?height=32&width=32" />
											<AvatarFallback>LC</AvatarFallback>
										</Avatar>
										<div className="flex-1 min-w-0">
											<p className="text-sm text-gray-900 font-medium">
												Node.js 18 is being discontinued on Monday, September
												1st, 2025
											</p>
											<p className="text-sm text-gray-600 mt-1">
												Please upgrade your Node.js version today to prevent any
												errors. Click for more information.
											</p>
											<p className="text-xs text-gray-500 mt-2">Aug 7</p>
										</div>
										<Button
											className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600 flex-shrink-0"
											size="sm"
											variant="ghost"
										>
											<svg
												className="h-4 w-4"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"
													strokeLinecap="round"
													strokeLinejoin="round"
													strokeWidth={2}
												/>
											</svg>
										</Button>
									</div>
								</div>

								<div className="px-4 py-3 border-b border-gray-100">
									<div className="flex items-center gap-3">
										<div className="h-8 w-8 rounded-full bg-orange-100 flex items-center justify-center flex-shrink-0">
											<AlertTriangle className="h-4 w-4 text-orange-600" />
										</div>
										<div className="flex-1 min-w-0">
											<p className="text-sm text-gray-900 font-medium">
												<span className="font-semibold">heroes-app</span> failed
												to deploy in the{" "}
												<span className="font-semibold">Preview</span>{" "}
												environment
											</p>
											<p className="text-xs text-gray-500 mt-1">Aug 6</p>
										</div>
										<Button
											className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600 flex-shrink-0"
											size="sm"
											variant="ghost"
										>
											<svg
												className="h-4 w-4"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"
													strokeLinecap="round"
													strokeLinejoin="round"
													strokeWidth={2}
												/>
											</svg>
										</Button>
									</div>
								</div>

								<div className="px-4 py-3">
									<div className="flex items-center gap-3">
										<Avatar className="h-8 w-8 flex-shrink-0">
											<AvatarImage src="/placeholder.svg?height=32&width=32" />
											<AvatarFallback>LC</AvatarFallback>
										</Avatar>
										<div className="flex-1 min-w-0">
											<p className="text-sm text-gray-900 font-medium">
												Node.js 18 is being discontinued on Monday, September
												1st, 2025
											</p>
											<p className="text-sm text-gray-600 mt-1">
												Please upgrade your Node.js version today to prevent any
												errors. Click for more information.
											</p>
										</div>
										<Button
											className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600 flex-shrink-0"
											size="sm"
											variant="ghost"
										>
											<svg
												className="h-4 w-4"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"
													strokeLinecap="round"
													strokeLinejoin="round"
													strokeWidth={2}
												/>
											</svg>
										</Button>
									</div>
								</div>
							</div>

							<div className="border-t border-gray-100 px-4 py-2">
								<Button
									className="w-full text-sm text-gray-600 hover:text-gray-900 justify-center"
									variant="ghost"
								>
									Archive All
								</Button>
							</div>
						</PopoverContent>
					</Popover>

					{/* Grid Icon */}
					<Button
						className="h-8 w-8 p-0 text-gray-600 hover:text-gray-900 hidden md:flex"
						size="sm"
						variant="ghost"
					>
						<Grid2X2 className="h-4 w-4" />
					</Button>

					{/* Hamburger Menu Button for Mobile */}
					<Button
						className="h-8 w-8 p-0 text-gray-600 hover:text-gray-900 md:hidden"
						size="sm"
						variant="ghost"
						onClick={() => {
							setIsMobileMenuOpen(true);
						}}
					>
						<svg
							className="h-4 w-4"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
						>
							<path
								d="M4 6h16M4 12h16M4 18h16"
								strokeLinecap="round"
								strokeLinejoin="round"
								strokeWidth={2}
							/>
						</svg>
					</Button>

					{/* User Avatar */}
					<Popover>
						<PopoverTrigger asChild>
							<Avatar className="h-8 w-8 cursor-pointer hidden md:block">
								<AvatarImage src="/placeholder.svg?height=32&width=32" />
								<AvatarFallback>LC</AvatarFallback>
							</Avatar>
						</PopoverTrigger>
						<PopoverContent align="end" className="w-80 p-0 z-50">
							<div className="p-4 border-b border-gray-100">
								<div className="font-medium text-gray-900">Luis Cusihuaman</div>
								<div className="text-sm text-gray-600">
									luiscusihuaman88@gmail.com
								</div>
							</div>
							<div className="p-2">
								<button className="flex items-center gap-3 px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<User className="h-4 w-4" />
									Dashboard
								</button>
								<button className="flex items-center gap-3 px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<Settings className="h-4 w-4" />
									Account Settings
								</button>
								<button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<div className="flex items-center gap-3">
										<Plus className="h-4 w-4" />
										Create Team
									</div>
									<Plus className="h-4 w-4 text-gray-400" />
								</button>
								<button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<div className="flex items-center gap-3">
										<Command className="h-4 w-4" />
										Command Menu
									</div>
									<div className="flex items-center gap-1">
										<kbd className="text-xs text-gray-500 bg-gray-100 px-1.5 py-0.5 rounded">
											⌘
										</kbd>
										<kbd className="text-xs text-gray-500 bg-gray-100 px-1.5 py-0.5 rounded">
											K
										</kbd>
									</div>
								</button>
								<button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<div className="flex items-center gap-3">
										<Monitor className="h-4 w-4" />
										Theme
									</div>
									<div className="flex items-center gap-1">
										<Monitor className="h-4 w-4 text-gray-400" />
										<Sun className="h-4 w-4 text-gray-400" />
										<Moon className="h-4 w-4 text-gray-400" />
									</div>
								</button>
								<button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<div className="flex items-center gap-3">
										<HomeIcon className="h-4 w-4" />
										Home Page
									</div>
									<svg
										className="text-gray-400"
										fill="currentColor"
										height="16"
										viewBox="0 0 75 65"
										width="16"
									>
										<path d="M37.59.25l36.95 64H.64l36.95-64z" />
									</svg>
								</button>
								<button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
									<div className="flex items-center gap-3">
										<LogOut className="h-4 w-4" />
										Log Out
									</div>
									<LogOut className="h-4 w-4 text-gray-400" />
								</button>
							</div>
							<div className="p-4 border-t border-gray-100">
								<Button className="w-full bg-black text-white hover:bg-gray-800">
									Upgrade to Pro
								</Button>
							</div>
						</PopoverContent>
					</Popover>
				</div>
			</header>
			{/* END: Header */}

			{/* START: MobileMenu */}
			{isMobileMenuOpen && (
				<div className="fixed inset-0 z-[9999] bg-white md:hidden">
					{/* Mobile Menu Header */}
					<div className="flex h-16 items-center justify-between border-b border-gray-200 px-4">
						<div className="flex items-center gap-3">
							<svg
								className="text-black"
								fill="currentColor"
								height="24"
								viewBox="0 0 75 65"
								width="24"
							>
								<path d="M37.59.25l36.95 64H.64l36.95-64z" />
							</svg>
							<div className="flex items-center gap-2">
								<Avatar className="h-7 w-7">
									<AvatarImage src="/placeholder.svg?height=28&width=28" />
									<AvatarFallback>LC</AvatarFallback>
								</Avatar>
								{isProjectView ? (
									<div className="flex items-center gap-2">
										{currentProject?.name === "relevo-app" ? (
											<div className="w-5 h-5 bg-purple-600 rounded flex items-center justify-center">
												<span className="text-white text-xs font-bold">V</span>
											</div>
										) : (
											<div className="w-5 h-5 bg-gray-400 rounded-full"></div>
										)}
										<span className="font-medium text-base text-gray-900">
											{currentProject?.name}
										</span>
									</div>
								) : (
									<span className="font-medium text-base text-gray-900">
										Luis Cusihuaman's projects
									</span>
								)}
								<ChevronDown className="h-4 w-4 text-gray-500" />
							</div>
						</div>
						<div className="flex items-center gap-2">
							<Button
								className="h-8 w-8 p-0 text-gray-600 hover:text-gray-900"
								size="sm"
								variant="ghost"
								onClick={() => {
									setIsMobileMenuOpen(false);
								}}
							>
								<svg
									className="h-4 w-4"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										d="M6 18L18 6M6 6l12 12"
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={2}
									/>
								</svg>
							</Button>
						</div>
					</div>

					{/* Mobile Menu Content */}
					<div className="flex flex-col h-[calc(100vh-4rem)] overflow-y-auto overscroll-contain touch-pan-y">
						<div className="p-4 space-y-4 pb-8">
							{/* Upgrade to Pro Button */}
							<Button className="w-full bg-black text-white hover:bg-gray-800 h-12 text-base font-medium">
								Upgrade to Pro
							</Button>

							{/* Contact Button */}
							<Button
								className="w-full h-12 text-base font-medium border-gray-300 bg-transparent"
								variant="outline"
							>
								Contact
							</Button>

							{/* User Profile Section */}
							<div className="flex items-center justify-between py-4 border-b border-gray-200">
								<div>
									<div className="font-medium text-gray-900">
										Luis Cusihuaman
									</div>
									<div className="text-sm text-gray-600">
										luiscusihuaman88@gmail.com
									</div>
								</div>
								<Avatar className="h-10 w-10">
									<AvatarImage src="/placeholder.svg?height=40&width=40" />
									<AvatarFallback>LC</AvatarFallback>
								</Avatar>
							</div>

							{/* Menu Items */}
							<div className="space-y-2">
								<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<div className="flex items-center gap-3">
										<Settings className="h-5 w-5 text-gray-500" />
										<span className="text-base">Account Settings</span>
									</div>
									<Settings className="h-5 w-5 text-gray-400" />
								</button>

								<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<div className="flex items-center gap-3">
										<Plus className="h-5 w-5 text-gray-500" />
										<span className="text-base">Create Team</span>
									</div>
									<Plus className="h-5 w-5 text-gray-400" />
								</button>

								<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<div className="flex items-center gap-3">
										<Monitor className="h-5 w-5 text-gray-500" />
										<span className="text-base">Theme</span>
									</div>
									<div className="flex items-center gap-1">
										<Monitor className="h-4 w-4 text-gray-400" />
										<Sun className="h-4 w-4 text-gray-400" />
										<Moon className="h-4 w-4 text-gray-400" />
									</div>
								</button>

								<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<div className="flex items-center gap-3">
										<LogOut className="h-5 w-5 text-gray-500" />
										<span className="text-base">Log Out</span>
									</div>
								</button>
							</div>

							{/* Resources Section */}
							<div className="pt-6 space-y-2">
								<h3 className="text-sm font-medium text-gray-500 px-4 mb-3">
									Resources
								</h3>

								<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<span className="text-base">Changelog</span>
								</button>

								<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<span className="text-base">Help</span>
								</button>

								<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<span className="text-base">Documentation</span>
								</button>

								<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
									<span className="text-base">Home page</span>
									<svg
										className="text-gray-400"
										fill="currentColor"
										height="16"
										viewBox="0 0 75 65"
										width="16"
									>
										<path d="M37.59.25l36.95 64H.64l36.95-64z" />
									</svg>
								</button>
							</div>
						</div>
					</div>
				</div>
			)}
			{/* END: MobileMenu */}

			{/* START: SearchOverlay */}
			{isSearchOpen && (
				<div
					className="fixed inset-0 z-[9999] bg-white/20 backdrop-blur-sm"
					onClick={() => {
						setIsSearchOpen(false);
					}}
				>
					<div
						className="absolute top-16 right-4 md:right-48 bg-white rounded-lg shadow-xl w-96 border border-gray-200"
						onClick={(event_) => {
							event_.stopPropagation();
						}}
					>
						{/* Functional search input header */}
						<div className="border-b border-gray-100 p-4">
							<div className="relative">
								<Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
								<Input
									autoFocus
									className="pl-10 pr-16 h-10 border-gray-300 focus:border-gray-400 focus:ring-0"
									placeholder="Find..."
									value={searchQuery}
									onChange={(event_) => {
										setSearchQuery(event_.target.value);
									}}
								/>
								<Button
									className="absolute right-2 top-1/2 transform -translate-y-1/2 text-xs text-gray-400 hover:text-gray-600"
									size="sm"
									variant="ghost"
									onClick={() => {
										setIsSearchOpen(false);
									}}
								>
									Esc
								</Button>
							</div>
						</div>

						{/* Search Results */}
						<div className="p-2 max-h-96 overflow-y-auto">
							{searchResults
								.filter(
									(result) =>
										searchQuery === "" ||
										result.name
											.toLowerCase()
											.includes(searchQuery.toLowerCase()) ||
										result.category
											.toLowerCase()
											.includes(searchQuery.toLowerCase())
								)
								.map((result, index) => (
									<div
										key={index}
										className="flex items-center gap-3 p-3 rounded-md hover:bg-gray-50 cursor-pointer"
									>
										<div className="flex-shrink-0">
											{result.type === "deployment" && (
												<div className="w-2 h-2 bg-green-500 rounded-full"></div>
											)}
											{result.type === "team" && (
												<img
													alt="Team"
													className="w-5 h-5 rounded-full"
													src="/placeholder.svg?height=20&width=20"
												/>
											)}
											{result.type === "project" &&
												result.name === "relevo-app" && (
													<div className="w-5 h-5 bg-purple-500 rounded flex items-center justify-center">
														<span className="text-white text-sm font-bold">
															V
														</span>
													</div>
												)}
											{result.type === "project" &&
												result.name === "Analytics" && (
													<div className="w-5 h-5 flex items-center justify-center">
														<svg
															className="w-4 h-4 text-gray-600"
															fill="none"
															stroke="currentColor"
															viewBox="0 0 24 24"
														>
															<path
																d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
																strokeLinecap="round"
																strokeLinejoin="round"
																strokeWidth={2}
															/>
														</svg>
													</div>
												)}
											{result.type === "assistant" && (
												<div className="w-5 h-5 flex items-center justify-center">
													<svg
														className="w-4 h-4 text-gray-600"
														fill="none"
														stroke="currentColor"
														viewBox="0 0 24 24"
													>
														<path
															d="M13 10V3L4 14h7v7l9-11h-7z"
															strokeLinecap="round"
															strokeLinejoin="round"
															strokeWidth={2}
														/>
													</svg>
												</div>
											)}
										</div>
										<div className="flex-1 min-w-0">
											<div className="font-medium text-gray-900 truncate">
												{result.name}
											</div>
											<div className="text-sm text-gray-500 truncate">
												{result.category}
											</div>
										</div>
									</div>
								))}
						</div>
					</div>
				</div>
			)}
			{/* END: SearchOverlay */}

			{/* START: SubNavigation */}
			<div className="sticky top-0 z-50 border-b border-gray-200 bg-white">
				<div className="px-4 md:px-6">
					<nav
						className="flex space-x-6 overflow-x-auto"
						style={{
							scrollbarWidth: "none",
							msOverflowStyle: "none",
						}}
					>
						{/* Tailwind-v4: Hide scrollbar cross-browser */}
						{/* Readable-JSX: Use Tailwind utilities instead of <style jsx> */}
						{/* 
							- 'scrollbar-none' is a Tailwind v4 utility for hiding scrollbars.
							- If not available, fallback to 'scrollbar-hide' from a plugin or custom CSS.
						*/}
						{(isProjectView
							? [
									"Overview",
									"Deployments",
									"Analytics",
									"Speed Insights",
									"Logs",
									"Observability",
									"Firewall",
									"Storage",
									"Flags",
									"Settings",
								]
							: [
									"Overview",
									"Integrations",
									"Deployments",
									"Activity",
									"Domains",
									"Usage",
									"Observability",
									"Storage",
									"Flags",
									"AI Gateway",
									"Support",
									"Settings",
								]
						).map((tab) => (
							<button
								key={tab}
								className={`text-sm transition-colors relative py-3 whitespace-nowrap flex-shrink-0 ${
									activeTab === tab
										? "text-gray-900 font-medium"
										: "text-gray-600 hover:text-gray-900 font-normal"
								}`}
								onClick={() => {
									if (tab === "Deployments" && !isProjectView) {
										window.location.href = "/deployments";
									} else {
										setActiveTab(tab);
									}
								}}
							>
								{tab}
								{activeTab === tab && (
									<div className="absolute bottom-0 left-0 right-0 h-0.5 bg-black" />
								)}
							</button>
						))}
					</nav>
				</div>
			</div>
			{/* END: SubNavigation */}

			{/* Main Content */}
			<div className="flex-1 p-6">
				{!isProjectView && activeTab === "Overview" && (
					<div className="space-y-6">
						{/* START: NodeVersionWarning */}
						<div className="bg-orange-50 border border-orange-200 rounded-lg p-4">
							<div className="flex items-start gap-3">
								<AlertTriangle className="h-5 w-5 text-orange-600 mt-0.5 flex-shrink-0" />
								<div className="flex-1">
									<p className="text-sm text-orange-800">
										<strong>
											You have 5 projects using Node.js 18 or older.
										</strong>{" "}
										These versions{" "}
										<a className="underline hover:no-underline" href="#">
											will be disabled soon
										</a>
										.
									</p>
								</div>
							</div>
						</div>
						{/* END: NodeVersionWarning */}

						<div className="max-w-7xl mx-auto px-6 py-6">
							{/* START: ProjectsToolbar */}
							<div className="flex items-center justify-between gap-4 mb-8">
								<div className="relative flex-1">
									<Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
									<Input
										className="pl-10 h-10 border-gray-300 focus:border-gray-400 focus:ring-0 bg-white"
										placeholder="Search Projects..."
									/>
								</div>
								<div className="flex items-center gap-2">
									<div className="flex items-center gap-1">
										<Button
											className="h-10 w-10 p-0 border-gray-300 bg-white hover:bg-gray-50"
											size="sm"
											variant="outline"
										>
											<Filter className="h-4 w-4" />
										</Button>
										<Button
											className="h-10 w-10 p-0 border-gray-300 bg-white hover:bg-gray-50"
											size="sm"
											variant="outline"
										>
											<Grid3X3 className="h-4 w-4" />
										</Button>
										<Button
											className="h-10 w-10 p-0 border-gray-300 bg-white hover:bg-gray-50"
											size="sm"
											variant="outline"
										>
											<List className="h-4 w-4" />
										</Button>
									</div>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<Button className="bg-black text-white hover:bg-gray-800 h-10 px-4 ml-2">
												Add New...
												<DropdownMenuChevronDown className="ml-2 h-4 w-4" />
											</Button>
										</DropdownMenuTrigger>
										<DropdownMenuContent>
											<DropdownMenuItem>Project</DropdownMenuItem>
											<DropdownMenuItem>Team</DropdownMenuItem>
										</DropdownMenuContent>
									</DropdownMenu>
								</div>
							</div>
							{/* END: ProjectsToolbar */}

							<div className="flex flex-col lg:flex-row gap-8">
								{/* START: DashboardSidebar */}
								<div className="lg:w-96 space-y-6">
									{/* START: UsageCard */}
									<div>
										<h2 className="text-base font-medium mb-4 leading-tight">
											Usage
										</h2>
										<div className="border border-gray-200 rounded-lg bg-white">
											<div className="p-6">
												<div className="flex items-center justify-between mb-6">
													<div>
														<p className="text-base font-medium text-gray-900 leading-tight">
															Last 30 days
														</p>
														<p className="text-sm text-gray-600 mt-1 leading-tight">
															Updated 16m ago
														</p>
													</div>
													<Button
														className="bg-black text-white hover:bg-gray-800 h-8 px-3 text-sm font-medium"
														size="sm"
													>
														Upgrade
													</Button>
												</div>

												<div className="space-y-3">
													<div className="flex items-center justify-between py-2">
														<div className="flex items-center gap-3">
															<div className="relative w-3 h-3">
																<svg
																	className="w-3 h-3 transform -rotate-90"
																	viewBox="0 0 36 36"
																>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#f3f4f6"
																		strokeWidth="4"
																	/>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#3b82f6"
																		strokeDasharray="2.34, 100"
																		strokeWidth="4"
																	/>
																</svg>
															</div>
															<span className="text-sm text-gray-900 leading-tight">
																Edge Requests
															</span>
														</div>
														<span className="text-sm text-gray-600 font-mono leading-tight">
															234 / 1M
														</span>
													</div>

													<div className="flex items-center justify-between py-2">
														<div className="flex items-center gap-3">
															<div className="relative w-3 h-3">
																<svg
																	className="w-3 h-3 transform -rotate-90"
																	viewBox="0 0 36 36"
																>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#f3f4f6"
																		strokeWidth="4"
																	/>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#3b82f6"
																		strokeDasharray="1.87, 100"
																		strokeWidth="4"
																	/>
																</svg>
															</div>
															<span className="text-sm text-gray-900 leading-tight">
																ISR Reads
															</span>
														</div>
														<span className="text-sm text-gray-600 font-mono leading-tight">
															187 / 1M
														</span>
													</div>

													<div className="flex items-center justify-between py-2">
														<div className="flex items-center gap-3">
															<div className="relative w-3 h-3">
																<svg
																	className="w-3 h-3 transform -rotate-90"
																	viewBox="0 0 36 36"
																>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#f3f4f6"
																		strokeWidth="4"
																	/>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#3b82f6"
																		strokeDasharray="1.34, 100"
																		strokeWidth="4"
																	/>
																</svg>
															</div>
															<span className="text-sm text-gray-900 leading-tight">
																Fast Origin Transfer
															</span>
														</div>
														<span className="text-sm text-gray-600 font-mono leading-tight">
															1.34 MB / 10 GB
														</span>
													</div>

													<div className="flex items-center justify-between py-2">
														<div className="flex items-center gap-3">
															<div className="relative w-3 h-3">
																<svg
																	className="w-3 h-3 transform -rotate-90"
																	viewBox="0 0 36 36"
																>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#f3f4f6"
																		strokeWidth="4"
																	/>
																	<path
																		d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
																		fill="none"
																		stroke="#e5e7eb"
																		strokeDasharray="4.33, 100"
																		strokeWidth="4"
																	/>
																</svg>
															</div>
															<span className="text-sm text-gray-900 leading-tight">
																Fast Data Transfer
															</span>
														</div>
														<span className="text-sm text-gray-600 font-mono leading-tight">
															4.33 MB / 100 GB
														</span>
													</div>

													<div className="flex justify-center pt-3">
														<Button
															className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600"
															size="sm"
															variant="ghost"
														>
															<ChevronDown className="h-4 w-4" />
														</Button>
													</div>
												</div>
											</div>
										</div>
									</div>
									{/* END: UsageCard */}

									{/* START: RecentPreviewsCard */}
									<div className="border border-gray-200 rounded-lg bg-white">
										<div className="p-6">
											<h3 className="text-base font-medium mb-4 leading-tight">
												Recent Previews
											</h3>
											<div className="space-y-0 divide-y divide-gray-100">
												{recentPreviews.map((preview, index) => (
													<div
														key={index}
														className="py-4 first:pt-0 last:pb-0"
													>
														<div className="flex items-center gap-3">
															<div className="flex -space-x-1 flex-shrink-0">
																{preview.avatars.map((avatar, index_) => (
																	<Avatar
																		key={index_}
																		className="h-6 w-6 border-2 border-white"
																	>
																		<AvatarImage
																			src={avatar.src || "/placeholder.svg"}
																		/>
																		<AvatarFallback
																			className={`${avatar.bg} text-white text-xs font-medium`}
																		>
																			{avatar.fallback}
																		</AvatarFallback>
																	</Avatar>
																))}
															</div>
															<div className="flex-1 min-w-0">
																<p className="text-sm text-gray-900 mb-2 leading-tight font-normal">
																	{preview.title}
																</p>
																<div className="flex items-center gap-2 flex-wrap">
																	<Button
																		className="h-6 px-2 text-xs text-gray-600 hover:text-gray-900 font-normal bg-gray-50 hover:bg-gray-100 rounded border border-gray-200"
																		size="sm"
																		variant="ghost"
																	>
																		<svg
																			className="h-3 w-3 mr-1"
																			fill="currentColor"
																			viewBox="0 0 20 20"
																		>
																			<path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
																			<path
																				clipRule="evenodd"
																				d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z"
																				fillRule="evenodd"
																			/>
																		</svg>
																		Preview
																	</Button>
																	{preview.status === "Source" && (
																		<Button
																			className="h-6 px-2 text-xs text-gray-600 hover:text-gray-900 font-normal bg-gray-50 hover:bg-gray-100 rounded border border-gray-200"
																			size="sm"
																			variant="ghost"
																		>
																			<Github className="h-3 w-3 mr-1" />
																			Source
																		</Button>
																	)}
																	{preview.pr && (
																		<span className="text-xs text-gray-500 font-normal">
																			{preview.pr}
																		</span>
																	)}
																	{preview.color && (
																		<Badge
																			className="text-xs h-5 px-2 bg-green-50 text-green-700 hover:bg-green-50 border-0 font-normal rounded"
																			variant="secondary"
																		>
																			{preview.color}
																		</Badge>
																	)}
																	{preview.status === "Error" && (
																		<Badge
																			className="text-xs h-5 px-2 bg-red-50 text-red-600 hover:bg-red-50 border-0 font-normal rounded"
																			variant="destructive"
																		>
																			● Error
																		</Badge>
																	)}
																</div>
															</div>
															<Button
																className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 flex-shrink-0 flex items-center justify-center"
																size="sm"
																variant="ghost"
															>
																<MoreHorizontal className="h-4 w-4" />
															</Button>
														</div>
													</div>
												))}
											</div>
										</div>
									</div>
									{/* END: RecentPreviewsCard */}
								</div>
								{/* END: DashboardSidebar */}

								{/* START: ProjectsList */}
								<div className="flex-1 min-w-0">
									<div className="mb-4">
										<h2 className="text-base font-medium leading-tight">
											Projects
										</h2>
									</div>
									<div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
										<ul className="divide-y divide-gray-200">
											{projects.map((project, index) => (
												<li
													key={index}
													className="grid grid-cols-[minmax(0,2fr)_minmax(0,2fr)_minmax(0,1fr)] items-center gap-6 py-4 px-6 hover:bg-gray-50 cursor-pointer transition-colors"
													onClick={() =>
														(window.location.href = `/${project.name}`)
													}
												>
													{/* Col 1 — Project */}
													<div className="flex items-center gap-3 min-w-0">
														<span className="h-10 w-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
															{project.name === "relevo-app" ||
															project.name === "psa-frontend" ? (
																<svg
																	fill="#6B7280"
																	height="20"
																	viewBox="0 0 75 65"
																	width="20"
																>
																	<path d="M37.59.25l36.95 64H.64l36.95-64z" />
																</svg>
															) : project.name === "calendar-app" ||
															  project.name === "heroes-app" ? (
																<svg
																	fill="#6B7280"
																	height="20"
																	viewBox="0 0 24 24"
																	width="20"
																>
																	<circle cx="12" cy="12" r="10" />
																	<path
																		d="M8 12h8"
																		stroke="currentColor"
																		strokeWidth="2"
																	/>
																	<path
																		d="M12 8v8"
																		stroke="currentColor"
																		strokeWidth="2"
																	/>
																</svg>
															) : project.name ===
															  "v0-portfolio-template-by-v0" ? (
																<div className="w-5 h-5 bg-gray-400 rounded-full"></div>
															) : project.name === "image-component" ||
															  project.name === "v0-music-game-concept" ? (
																<div className="w-6 h-6 bg-gray-600 rounded-full flex items-center justify-center">
																	<span className="text-white text-xs font-bold">
																		N
																	</span>
																</div>
															) : (
																<svg
																	fill="#6B7280"
																	height="20"
																	viewBox="0 0 75 65"
																	width="20"
																>
																	<path d="M37.59.25l36.95 64H.64l36.95-64z" />
																</svg>
															)}
														</span>
														<div className="min-w-0 flex-1">
															<div className="text-sm font-medium text-gray-900 truncate">
																{project.name}
															</div>
															<div className="text-xs text-gray-600 truncate">
																{project.url}
															</div>
														</div>
													</div>

													{/* Col 2 — Activity */}
													<div className="min-w-0 flex-1">
														{project.status === "Connect Git Repository" ? (
															<a className="block text-sm font-medium text-blue-600 hover:text-blue-700 hover:underline truncate">
																{project.status}
															</a>
														) : (
															<div className="block text-sm font-medium text-gray-900 truncate">
																{project.status}
															</div>
														)}
														<div className="mt-1 text-xs text-gray-600 flex items-center gap-2">
															<time>{project.date}</time>
															{project.branch && (
																<>
																	<span>on</span>
																	<span className="inline-flex items-center gap-1">
																		<GitBranch className="h-3.5 w-3.5" />
																		{project.branch}
																	</span>
																</>
															)}
														</div>
													</div>

													{/* Col 3 — Repo chip + Health + ⋯ */}
													<div className="flex items-center justify-end gap-2 shrink-0 min-w-0">
														{project.hasGithub ? (
															<a className="inline-flex items-center h-7 px-2.5 rounded-full border border-gray-200 bg-gray-50 text-gray-700 text-xs min-w-0 max-w-[120px] truncate">
																<Github className="h-3.5 w-3.5 mr-1.5 shrink-0" />
																<span className="truncate">
																	{project.github}
																</span>
															</a>
														) : (
															<div className="w-[120px]"></div>
														)}
														<button className="h-8 w-8 rounded-full text-gray-400 hover:bg-gray-50 shrink-0 flex items-center justify-center">
															<Activity className="h-4 w-4" />
														</button>
														<DropdownMenu>
															<DropdownMenuTrigger asChild>
																<button className="h-8 w-8 rounded-full text-gray-600 hover:bg-gray-50 shrink-0 flex items-center justify-center">
																	<MoreHorizontal className="h-4 w-4" />
																</button>
															</DropdownMenuTrigger>
															<DropdownMenuContent align="end">
																<DropdownMenuItem>
																	View Project
																</DropdownMenuItem>
																<DropdownMenuItem>Settings</DropdownMenuItem>
																<DropdownMenuItem className="text-red-600">
																	Delete
																</DropdownMenuItem>
															</DropdownMenuContent>
														</DropdownMenu>
													</div>
												</li>
											))}
										</ul>
									</div>
								</div>
								{/* END: ProjectsList */}
							</div>
						</div>
					</div>
				)}

				{!isProjectView && activeTab === "Deployments" && (
					<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
						{/* START: DeploymentsHeader */}
						<div className="flex items-center justify-between">
							<div>
								<h1 className="text-2xl font-semibold text-gray-900">
									Deployments
								</h1>
								<p className="text-sm text-gray-600 mt-1">
									All deployments from{" "}
									<code className="bg-gray-100 px-1 py-0.5 rounded text-xs">
										luis-cusihuamans-projects
									</code>
								</p>
							</div>
						</div>
						{/* END: DeploymentsHeader */}

						{/* START: DeploymentsToolbar */}
						<div className="flex items-center justify-between mt-6 mb-4">
							<div className="flex items-center gap-4">
								<button className="flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">
									<Calendar className="h-4 w-4" />
									Select Date Range
								</button>
							</div>
							<div className="flex items-center gap-4">
								<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
									<option>All Environments</option>
									<option>Production</option>
									<option>Preview</option>
								</select>
								<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
									<option>Status 5/6</option>
									<option>Ready</option>
									<option>Error</option>
								</select>
							</div>
						</div>
						{/* END: DeploymentsToolbar */}

						{/* START: DeploymentsTable */}
						<div className="hidden md:block rounded-lg border border-gray-200 bg-white overflow-hidden">
							{[
								{
									id: "CDMFQKRs",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "calendar-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "c5b235d",
									message: "chore(deps): bump dependencies",
									time: "2d ago",
									author: "dependabot[bot]",
									statusTime: "2s (2d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "8LB4tSUAh",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "calendar-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "7d4dbb5",
									message: "chore(deps): bump dependencies",
									time: "3d ago",
									author: "dependabot[bot]",
									statusTime: "2s (3d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "3L5k6ngCp",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "heroes-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "7a50c77",
									message: "chore(deps): bump dependencies",
									time: "Aug 6",
									author: "dependabot[bot]",
									statusTime: "4s (18d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "GX6A8fhaZ",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "calendar-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "1348002",
									message: "chore(deps): bump dependencies",
									time: "Jul 17",
									author: "dependabot[bot]",
									statusTime: "4s (38d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "6qYUWvuN3",
									status: "Ready",
									statusColor: "bg-green-500",
									environment: "Promoted",
									environmentColor: "text-gray-600",
									project: "relevo-app",
									projectIcon: {
										type: "text",
										value: "V",
										bg: "bg-purple-500",
										text: "text-white",
									},
									branch: "main",
									commit: "Current",
									message: "Production Rebuild of 8Td...",
									time: "Jul 12",
									author: "luiscusihuaman",
									statusTime: "29s (43d ago)",
									current: true,
									environmentType: "Production",
									avatar: "LC",
								},
								{
									id: "8TdfXLHgY",
									status: "Ready",
									statusColor: "bg-green-500",
									environment: "Staged",
									environmentColor: "text-gray-600",
									project: "relevo-app",
									projectIcon: {
										type: "text",
										value: "V",
										bg: "bg-purple-500",
										text: "text-white",
									},
									branch: "eduardoc/spanish",
									commit: "e78260a",
									message: "Enhance diabetes management...",
									time: "Jul 12",
									author: "luiscusihuaman",
									statusTime: "21s (43d ago)",
									environmentType: "Preview",
									avatar: "LC",
								},
							].map((deployment, index) => (
								<div
									key={deployment.id}
									className={`grid grid-cols-[1fr_1fr_1fr_1fr_1fr] items-center gap-4 py-3 px-4 hover:bg-gray-50 transition-colors cursor-pointer ${
										index < 5 ? "border-b border-gray-100" : ""
									}`}
									onClick={() => {
										handleDeploymentClick(deployment.id, deployment.project);
									}}
								>
									{/* Deployment Column */}
									<div className="min-w-0">
										<div className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate">
											{deployment.id}
										</div>
										<div className="text-xs text-gray-500 mt-0.5">
											{deployment.environmentType}
										</div>
									</div>

									{/* Status Column */}
									<div className="min-w-0">
										<div className="flex items-center gap-2 mb-1">
											<span
												className={`h-2 w-2 rounded-full ${deployment.statusColor}`}
											></span>
											<span className="text-sm font-medium text-gray-900">
												{deployment.status}
											</span>
										</div>
										<div className="text-xs text-gray-500">
											{deployment.statusTime}
										</div>
									</div>

									{/* Environment Column */}
									<div className="min-w-0">
										<div
											className={`text-sm ${deployment.environmentColor} font-medium`}
										>
											{deployment.environment}
										</div>
										<div className="text-xs text-gray-500 mt-0.5">
											{deployment.time}
										</div>
									</div>

									{/* Source Column */}
									<div className="min-w-0">
										<div className="flex items-center gap-2 mb-1">
											<div
												className={`w-5 h-5 rounded-full flex items-center justify-center text-xs ${deployment.projectIcon.bg} ${deployment.projectIcon.text || "text-gray-700"}`}
											>
												{deployment.projectIcon.value}
											</div>
											<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate">
												{deployment.project}
											</span>
										</div>
										<div className="flex items-center gap-1 text-xs text-gray-500">
											<GitBranch className="h-3 w-3" />
											<span className="truncate">{deployment.branch}</span>
										</div>
										<div className="flex items-center gap-1 mt-0.5">
											<span className="font-mono text-xs bg-gray-100 px-1 py-0.5 rounded text-gray-700">
												{deployment.commit}
											</span>
											<span className="text-xs text-gray-500 truncate">
												{deployment.message}
											</span>
										</div>
									</div>

									{/* Created Column */}
									<div className="min-w-0 text-right">
										<div className="flex items-center justify-end gap-2">
											<GitBranch className="h-3 w-3 text-gray-400" />
											<span className="text-xs text-gray-500">
												{deployment.time} by {deployment.author}
											</span>
											<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
												{deployment.avatar}
											</div>
										</div>
									</div>
								</div>
							))}
						</div>
						{/* END: DeploymentsTable */}

						{/* START: DeploymentsMobileList */}
						<div className="md:hidden space-y-4">
							{[
								{
									id: "CDMFQKRs",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "calendar-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "c5b235d",
									message: "chore(deps): bump dependencies",
									time: "2d ago",
									author: "dependabot[bot]",
									statusTime: "2s (2d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "8LB4tSUAh",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "calendar-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "7d4dbb5",
									message: "chore(deps): bump dependencies",
									time: "3d ago",
									author: "dependabot[bot]",
									statusTime: "2s (3d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "3L5k6ngCp",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "heroes-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "7a50c77",
									message: "chore(deps): bump dependencies",
									time: "Aug 6",
									author: "dependabot[bot]",
									statusTime: "4s (18d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "GX6A8fhaZ",
									status: "Error",
									statusColor: "bg-red-500",
									environment: "Unexpected Error",
									environmentColor: "text-red-600",
									project: "calendar-app",
									projectIcon: {
										type: "emoji",
										value: "❄️",
										bg: "bg-blue-100",
									},
									branch: "dependabot/npm_and_yarn",
									commit: "1348002",
									message: "chore(deps): bump dependencies",
									time: "Jul 17",
									author: "dependabot[bot]",
									statusTime: "4s (38d ago)",
									environmentType: "Preview",
									avatar: "DB",
								},
								{
									id: "6qYUWvuN3",
									status: "Ready",
									statusColor: "bg-green-500",
									environment: "Promoted",
									environmentColor: "text-gray-600",
									project: "relevo-app",
									projectIcon: {
										type: "text",
										value: "V",
										bg: "bg-purple-500",
										text: "text-white",
									},
									branch: "main",
									commit: "Current",
									message: "Production Rebuild of 8Td...",
									time: "Jul 12",
									author: "luiscusihuaman",
									statusTime: "29s (43d ago)",
									current: true,
									environmentType: "Production",
									avatar: "LC",
								},
								{
									id: "8TdfXLHgY",
									status: "Ready",
									statusColor: "bg-green-500",
									environment: "Staged",
									environmentColor: "text-gray-600",
									project: "relevo-app",
									projectIcon: {
										type: "text",
										value: "V",
										bg: "bg-purple-500",
										text: "text-white",
									},
									branch: "eduardoc/spanish",
									commit: "e78260a",
									message: "Enhance diabetes management...",
									time: "Jul 12",
									author: "luiscusihuaman",
									statusTime: "21s (43d ago)",
									environmentType: "Preview",
									avatar: "LC",
								},
							].map((deployment) => (
								<div
									key={deployment.id}
									className="bg-white border border-gray-200 rounded-lg p-4 space-y-3 cursor-pointer hover:border-gray-300 transition-colors"
									onClick={() => {
										handleDeploymentClick(deployment.id, deployment.project);
									}}
								>
									{/* Card Header */}
									<div className="flex items-center justify-between">
										<div>
											<h3 className="font-medium text-gray-900 text-base hover:underline cursor-pointer">
												{deployment.id}
											</h3>
											<p className="text-sm text-gray-500">
												{deployment.environmentType}
											</p>
										</div>
										{deployment.current && (
											<span className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full font-medium">
												Current
											</span>
										)}
									</div>

									{/* Status */}
									<div className="flex items-center gap-2">
										<span
											className={`h-2 w-2 rounded-full ${deployment.statusColor}`}
										></span>
										<span className="text-sm font-medium text-gray-900">
											{deployment.status}
										</span>
										<span className="text-sm text-gray-500">
											{deployment.statusTime}
										</span>
									</div>

									{/* Environment */}
									<div>
										<div
											className={`text-sm font-medium ${deployment.environmentColor}`}
										>
											{deployment.environment}
										</div>
										<div className="text-sm text-gray-500">
											{deployment.time}
										</div>
									</div>

									{/* Project Info */}
									<div className="flex items-center gap-2">
										<div
											className={`w-6 h-6 rounded-full flex items-center justify-center text-sm ${deployment.projectIcon.bg} ${deployment.projectIcon.text || "text-gray-700"}`}
										>
											{deployment.projectIcon.value}
										</div>
										<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer">
											{deployment.project}
										</span>
									</div>

									{/* Source Info */}
									<div className="space-y-1">
										<div className="flex items-center gap-1 text-sm text-gray-600">
											<GitBranch className="h-4 w-4" />
											<span>{deployment.branch}</span>
										</div>
										<div className="flex items-center gap-2">
											<span className="font-mono text-sm bg-gray-100 px-2 py-1 rounded text-gray-700">
												{deployment.commit}
											</span>
											<span className="text-sm text-gray-600">
												{deployment.message}
											</span>
										</div>
									</div>

									{/* Author */}
									<div className="flex items-center justify-between pt-2 border-t border-gray-100">
										<div className="flex items-center gap-2">
											<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
												{deployment.avatar}
											</div>
											<span className="text-sm text-gray-600">
												{deployment.time} by {deployment.author}
											</span>
										</div>
									</div>
								</div>
							))}
						</div>
						{/* END: DeploymentsMobileList */}
					</div>
				)}

				{isProjectView ? (
					<div className="space-y-6">
						{activeTab === "Overview" && (
							<>
								{/* START: ProjectHeader */}
								<div>
									<h1 className="text-2xl font-semibold text-gray-900 mb-8">
										{currentProject?.name}
									</h1>

									<div className="bg-white border border-gray-200 rounded-xl p-6">
										<div className="flex items-center justify-between mb-6">
											<h2 className="text-lg font-medium">
												Production Deployment
											</h2>
											<div className="flex items-center gap-3">
												<button className="px-3 py-1.5 text-sm border border-gray-200 rounded-md hover:bg-gray-50">
													Build Logs
												</button>
												<button className="px-3 py-1.5 text-sm border border-gray-200 rounded-md hover:bg-gray-50">
													Runtime Logs
												</button>
												<button className="px-3 py-1.5 text-sm border border-gray-200 rounded-md hover:bg-gray-50 flex items-center gap-2">
													<RotateCcw className="h-4 w-4" />
													Instant Rollback
												</button>
											</div>
										</div>

										<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
											<div className="bg-gray-50 rounded-lg p-4 flex items-center justify-center h-64">
												<div className="text-center">
													<div className="w-16 h-16 bg-purple-100 rounded-lg flex items-center justify-center mx-auto mb-3">
														<div className="w-8 h-8 bg-purple-600 rounded flex items-center justify-center">
															<span className="text-white text-sm font-bold">
																V
															</span>
														</div>
													</div>
													<p className="text-sm text-gray-600">
														Configuración de RELEVO
													</p>
												</div>
											</div>

											<div className="space-y-4">
												<div>
													<h3 className="text-sm font-medium text-gray-700 mb-1">
														Deployment
													</h3>
													<p className="text-sm text-gray-900">
														{currentProject?.name}
														-1a70w6d3y-luis-cusihuamans-projects.vercel.app
													</p>
												</div>
											</div>
										</div>
									</div>
								</div>
								{/* END: ProjectHeader */}
							</>
						)}
					</div>
				) : null}
			</div>
		</div>
	);
}
