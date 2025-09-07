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
	ChevronRight,
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

import type { Patient } from "./types";
import { useUserStore } from "@/store/user.store";
import { useTranslation } from "react-i18next";
import { NotificationsPopover } from "@/components/home/NotificationsPopover";

type AppHeaderProps = {
	isPatientView: boolean;
	currentPatient: Patient | null;
	setIsSearchOpen: (isOpen: boolean) => void;
	isMobileMenuOpen: boolean;
	setIsMobileMenuOpen: (isOpen: boolean) => void;
};

export const AppHeader: FC<AppHeaderProps> = ({
	isPatientView,
	currentPatient,
	setIsSearchOpen,
	isMobileMenuOpen,
	setIsMobileMenuOpen,
}) => {
	const { doctorName, unitName } = useUserStore();
    const { t } = useTranslation("home");
	return (
		<header className="flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 md:px-6">
			<div className="flex items-center gap-3">
				{/* Vercel Logo */}
				<div className="flex items-center gap-3">
					<svg fill="currentColor" height="24" viewBox="0 0 75 65" width="24">
						<path d="M37.59.25l36.95 64H.64l36.95-64z" />
					</svg>

					<div className="flex items-center gap-2">
						{isPatientView ? (
							<div className="flex items-center gap-2">
								<button
									className="text-base font-medium text-gray-900 hover:text-gray-700"
									onClick={() => (window.location.href = "/")}
								>
									Relevo de Luis Cusihuaman
								</button>
								<ChevronRight className="h-4 w-4 text-gray-400" />
								<div className="flex items-center gap-2">
									<div className="w-5 h-5 bg-purple-600 rounded flex items-center justify-center">
										<span className="text-white text-xs font-bold">v</span>
									</div>
									<span className="text-base font-medium text-gray-900">
										{unitName || "UCIP"}
									</span>
								</div>
								<ChevronRight className="h-4 w-4 text-gray-400" />
								<span className="text-base font-medium text-gray-900">
									{currentPatient?.name}
								</span>
							</div>
						) : (
							<div className="flex items-center gap-2">
								<span className="text-base font-medium text-gray-900">
									Relevo de {doctorName || "Luis Cusihuaman"}
								</span>
								<ChevronRight className="h-4 w-4 text-gray-400" />
								<div className="w-5 h-5 bg-purple-600 rounded flex items-center justify-center">
									<span className="text-white text-xs font-bold">v</span>
								</div>
								<span className="text-base font-medium text-gray-900">{unitName || "UCIP"}</span>
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
						placeholder={t("search.placeholder")}
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
					{t("feedback.suggestions")}
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
				<NotificationsPopover isMobileMenuOpen={isMobileMenuOpen} />

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
