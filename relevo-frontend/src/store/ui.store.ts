import { create } from "zustand";
import type { Patient } from "@/components/home/types";

type UiState = {
	isSearchOpen: boolean;
	isMobileMenuOpen: boolean;
	currentPatient: Patient | null;
	actions: {
		setIsSearchOpen: (isOpen: boolean) => void;
		setIsMobileMenuOpen: (isOpen: boolean) => void;
		setCurrentPatient: (patient: Patient | null) => void;
	};
};

export const useUiStore = create<UiState>((set) => ({
	isSearchOpen: false,
	isMobileMenuOpen: false,
	currentPatient: null,
	actions: {
		setIsSearchOpen: (isOpen: boolean): void => {
			set({ isSearchOpen: isOpen });
		},
		setIsMobileMenuOpen: (isOpen: boolean): void => {
			set({ isMobileMenuOpen: isOpen });
		},
		setCurrentPatient: (patient: Patient | null): void => {
			set({ currentPatient: patient });
		},
	},
}));
