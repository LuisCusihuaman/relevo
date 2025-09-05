import type { FC } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
	ChevronDown,
	Settings,
	Plus,
	Monitor,
	Sun,
	Moon,
	LogOut,
} from "lucide-react";

import type { Project } from "./types";

type MobileMenuProps = {
	isProjectView: boolean;
	currentProject: Project | null;
	setIsMobileMenuOpen: (isOpen: boolean) => void;
};

export const MobileMenu: FC<MobileMenuProps> = ({
	isProjectView,
	currentProject,
	setIsMobileMenuOpen,
}) => {
	return (
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
	);
};
