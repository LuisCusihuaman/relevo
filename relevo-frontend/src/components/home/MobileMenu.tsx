import type { FC } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
	ChevronRight,
	Settings,
	Plus,
	Monitor,
	Sun,
	Moon,
	LogOut,
} from "lucide-react";
import { useUser } from "@clerk/clerk-react";
import { useSignOut } from "@/hooks/useSignOut";

import type { PatientHandoverData } from "@/types/domain";
import { useTranslation } from "react-i18next";

type MobileMenuProps = {
	isPatientView: boolean;
	currentPatient: PatientHandoverData | null;
	setIsMobileMenuOpen: (isOpen: boolean) => void;
};

export const MobileMenu: FC<MobileMenuProps> = ({
	isPatientView,
	currentPatient,
	setIsMobileMenuOpen,
}) => {
    const { user } = useUser();
    const { signOut } = useSignOut();
    const { t } = useTranslation("home");

    // Get unit name from localStorage (saved after shift check-in configuration)
    const unitName = typeof window !== "undefined" 
		? (window.localStorage.getItem("selectedUnitName") || "UCIP")
		: "UCIP";
    const displayName = user?.fullName || "";
    const primaryEmail = user?.primaryEmailAddress?.emailAddress ?? user?.emailAddresses?.[0]?.emailAddress ?? "";
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
						{isPatientView ? (
							<div className="flex items-center gap-2">
								<span className="font-medium text-base text-gray-900">
									Relevo de {displayName || "Doctor"}
								</span>
								<ChevronRight className="h-4 w-4 text-gray-500" />
								<div className="w-5 h-5 bg-purple-600 rounded flex items-center justify-center">
									<span className="text-white text-xs font-bold">v</span>
								</div>
								<span className="font-medium text-base text-gray-900">
									{unitName || "UCIP"}
								</span>
								<ChevronRight className="h-4 w-4 text-gray-500" />
								<span className="font-medium text-base text-gray-900">
									{currentPatient?.name}
								</span>
							</div>
						) : (
							<div className="flex items-center gap-2">
								<span className="font-medium text-base text-gray-900">
									Relevo de {displayName || "Doctor"}
								</span>
								<ChevronRight className="h-4 w-4 text-gray-500" />
								<div className="w-5 h-5 bg-purple-600 rounded flex items-center justify-center">
									<span className="text-white text-xs font-bold">v</span>
								</div>
								<span className="font-medium text-base text-gray-900">{unitName || "UCIP"}</span>
								<ChevronRight className="h-4 w-4 text-gray-500" />
								{currentPatient && (
									<span className="font-medium text-base text-gray-900">
										{currentPatient.name}
									</span>
								)}
							</div>
						)}
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
					{/* Contact Button */}
					<Button className="w-full h-12 text-base font-medium border-gray-300 bg-transparent" variant="outline">
						{t("userMenu.contact")}
					</Button>

					{/* User Profile Section */}
					<div className="flex items-center justify-between py-4 border-b border-gray-200">
						<div>
							<div className="font-medium text-gray-900">{displayName}</div>
							<div className="text-sm text-gray-600">{primaryEmail}</div>
						</div>
						<Avatar className="h-10 w-10">
							<AvatarImage src={user?.imageUrl} />
							<AvatarFallback>{user?.firstName?.[0] || displayName?.[0] || "U"}</AvatarFallback>
						</Avatar>
					</div>

					{/* Menu Items */}
					<div className="space-y-2">
						<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<div className="flex items-center gap-3">
								<Settings className="h-5 w-5 text-gray-500" />
								<span className="text-base">{t("userMenu.accountSettings")}</span>
							</div>
							<Settings className="h-5 w-5 text-gray-400" />
						</button>

						<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<div className="flex items-center gap-3">
								<Plus className="h-5 w-5 text-gray-500" />
								<span className="text-base">{t("userMenu.createTeam")}</span>
							</div>
							<Plus className="h-5 w-5 text-gray-400" />
						</button>

						<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<div className="flex items-center gap-3">
								<Monitor className="h-5 w-5 text-gray-500" />
								<span className="text-base">{t("userMenu.theme")}</span>
							</div>
							<div className="flex items-center gap-1">
								<Monitor className="h-4 w-4 text-gray-400" />
								<Sun className="h-4 w-4 text-gray-400" />
								<Moon className="h-4 w-4 text-gray-400" />
							</div>
						</button>

						<button
							className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg"
							onClick={() => signOut()}
						>
							<div className="flex items-center gap-3">
								<LogOut className="h-5 w-5 text-gray-500" />
								<span className="text-base">{t("userMenu.logOut")}</span>
							</div>
						</button>
					</div>

					{/* Resources Section */}
					<div className="pt-6 space-y-2">
						<h3 className="text-sm font-medium text-gray-500 px-4 mb-3">{t("userMenu.resources")}</h3>

						<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<span className="text-base">{t("userMenu.changelog")}</span>
						</button>

						<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<span className="text-base">{t("userMenu.help")}</span>
						</button>

						<button className="flex items-center w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<span className="text-base">{t("userMenu.documentation")}</span>
						</button>

						<button className="flex items-center justify-between w-full px-4 py-3 text-left text-gray-700 hover:bg-gray-50 rounded-lg">
							<span className="text-base">{t("userMenu.homePage")}</span>
							<svg className="text-gray-400" fill="currentColor" height="16" viewBox="0 0 75 65" width="16">
								<path d="M37.59.25l36.95 64H.64l36.95-64z" />
							</svg>
						</button>
					</div>
				</div>
			</div>
		</div>
	);
};
