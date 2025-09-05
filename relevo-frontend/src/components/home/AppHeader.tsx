import type { FC } from "react";
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
	HomeIcon,
} from "lucide-react";

import type { Project } from "./types";

type AppHeaderProps = {
	isProjectView: boolean;
	currentProject: Project | null;
	setIsSearchOpen: (isOpen: boolean) => void;
	isMobileMenuOpen: boolean;
	setIsMobileMenuOpen: (isOpen: boolean) => void;
};

export const AppHeader: FC<AppHeaderProps> = ({
	isProjectView,
	currentProject,
	setIsSearchOpen,
	isMobileMenuOpen,
	setIsMobileMenuOpen,
}) => {
	return (
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
					size="sm"
					variant="ghost"
					className={`h-8 w-8 p-0 text-gray-600 hover:text-gray-900 md:hidden ${
						isMobileMenuOpen ? "hidden" : ""
					}`}
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
											<span className="font-semibold">calendar-app</span> failed
											to deploy in the{" "}
											<span className="font-semibold">Preview</span> environment
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
											<span className="font-semibold">calendar-app</span> failed
											to deploy in the{" "}
											<span className="font-semibold">Preview</span> environment
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
											<span className="font-semibold">Preview</span> environment
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
	);
};
