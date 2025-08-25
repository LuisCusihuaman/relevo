import { create } from "zustand";

type UserState = {
	doctorName: string;
	setDoctorName: (name: string) => void;
};

export const useUserStore = create<UserState>((set) => ({
	doctorName: "",
	setDoctorName: (name: string): void => {
		set({ doctorName: name });
	},
}));
