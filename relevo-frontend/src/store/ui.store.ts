import { create } from "zustand";
import type { PatientHandoverData } from "@/types/domain";

type UiState = {
	isSearchOpen: boolean;
	isMobileMenuOpen: boolean;
	currentPatient: PatientHandoverData | null;
	actions: {
		setIsSearchOpen: (isOpen: boolean) => void;
		setIsMobileMenuOpen: (isOpen: boolean) => void;
		setCurrentPatient: (patient: PatientHandoverData | null) => void;
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
		setCurrentPatient: (patient: PatientHandoverData | null): void => {
			set({ currentPatient: patient });
		},
	},
}));
