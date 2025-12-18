import type { FC } from "react";
import { ChevronRight } from "lucide-react";
import { useUser } from "@clerk/clerk-react";

import type { PatientHandoverData } from "@/types/domain";
import { useTranslation } from "react-i18next";
import { UserMenuPopover } from "./UserMenuPopover";

type AppHeaderProps = {
	isPatientView: boolean;
	currentPatient: PatientHandoverData | null;
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
	const { user } = useUser();
	const { t } = useTranslation("home");

	const doctorName = user?.fullName || "Doctor";
	// Get unit name from localStorage (saved after shift check-in configuration)
	const unitName = typeof window !== "undefined" 
		? (window.localStorage.getItem("selectedUnitName") || "UCIP")
		: "UCIP";

	// Debug logs
	console.log("AppHeader Debug Info:");
	console.log("- isPatientView:", isPatientView);
	console.log("- currentPatient:", currentPatient);
	console.log("- doctorName:", doctorName);
	console.log("- unitName:", unitName);
	console.log("- user:", user);
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
									Relevo de {doctorName || "Doctor"}
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
									Relevo de {doctorName || "Doctor"}
								</span>
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
								{currentPatient && (
									<span className="text-base font-medium text-gray-900">
										{currentPatient.name}
									</span>
								)}
							</div>
						)}
					</div>
				</div>
			</div>

			<div className="flex items-center gap-2 md:gap-4">
				{/* User Avatar + Mobile Menu */}
				<UserMenuPopover
					onOpenMobileMenu={() => {
						setIsMobileMenuOpen(true);
					}}
				/>
			</div>
		</header>
	);
};
