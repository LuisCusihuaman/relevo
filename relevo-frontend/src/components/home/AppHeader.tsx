import type { FC } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Search, ChevronRight, Grid2X2 } from "lucide-react";

import type { Patient } from "./types";
import { useUserStore } from "@/store/user.store";
import { useTranslation } from "react-i18next";
import { NotificationsPopover } from "./NotificationsPopover";
import { UserMenuPopover } from "./UserMenuPopover";

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
						placeholder={t("search.placeholder") as string}
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

				{/* User Avatar + Mobile Menu */}
				<UserMenuPopover onOpenMobileMenu={() => { setIsMobileMenuOpen(true); }} />
			</div>
		</header>
	);
};
