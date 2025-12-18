import { type ReactElement, useEffect, useState } from "react";
import { Outlet } from "@tanstack/react-router";
import {
	AppHeader,
	CommandPalette,
	MobileMenu,
	SubNavigation,
} from "@/components/home";
import { searchResults } from "@/pages/data";
import { useUiStore } from "@/store/ui.store";

export function AppLayout(): ReactElement {
	const [searchQuery, setSearchQuery] = useState<string>("");
	const { isSearchOpen, isMobileMenuOpen, currentPatient, actions } =
		useUiStore();
	const isPatientView = Boolean(currentPatient);

	useEffect(() => {
		const handleKeyDown = (event_: KeyboardEvent): void => {
			if (event_.key === "Escape") {
				actions.setIsSearchOpen(false);
			}
		};
		document.addEventListener("keydown", handleKeyDown);
		return (): void => {
			document.removeEventListener("keydown", handleKeyDown);
		};
	}, [actions]);

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

	return (
		<div className="min-h-screen bg-gray-50">
			<AppHeader
				currentPatient={currentPatient}
				isMobileMenuOpen={isMobileMenuOpen}
				isPatientView={isPatientView}
				setIsMobileMenuOpen={actions.setIsMobileMenuOpen}
				setIsSearchOpen={actions.setIsSearchOpen}
			/>

			{isMobileMenuOpen && (
				<MobileMenu
					currentPatient={currentPatient}
					isPatientView={isPatientView}
					setIsMobileMenuOpen={actions.setIsMobileMenuOpen}
				/>
			)}

			{isSearchOpen && (
				<CommandPalette
					searchQuery={searchQuery}
					searchResults={searchResults}
					setIsSearchOpen={actions.setIsSearchOpen}
					setSearchQuery={setSearchQuery}
				/>
			)}

			<SubNavigation />
			<Outlet />
		</div>
	);
}

// Setup Layout - Minimal layout for setup/wizard flows
export function SetupLayout(): ReactElement {
	return (
		<div className="min-h-screen bg-background">
			<Outlet />
		</div>
	);
}
