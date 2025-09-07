import { create } from "zustand";

type UserState = {
	doctorName: string;
	unitName: string;
	setDoctorName: (name: string) => void;
	setUnitName: (unit: string) => void;
};

export const useUserStore = create<UserState>((set) => ({
	doctorName: "",
	unitName: "",
	setDoctorName: (name: string): void => {
		set({ doctorName: name });
	},
	setUnitName: (unit: string): void => {
		set({ unitName: unit });
	},
}));
