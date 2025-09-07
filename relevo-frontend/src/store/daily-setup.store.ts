import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { SetupState } from "@/features/daily-setup";

type DailySetupState = Pick<SetupState, "currentStep" | "doctorName" | "unit" | "shift" | "selectedIndexes">;

type DailySetupActions = {
	setState: (newState: Partial<DailySetupState>) => void;
	reset: () => void;
};

const initialState: DailySetupState = {
	currentStep: 0,
	doctorName: "",
	unit: "",
	shift: "",
	selectedIndexes: [],
};

export const useDailySetupStore = create<DailySetupState & DailySetupActions>()(
	persist(
		(set) => ({
			...initialState,
			setState: (newState) => set((state) => ({ ...state, ...newState })),
			reset: () => set(initialState),
		}),
		{
			name: "daily-setup-storage", // unique name for localStorage key
		},
	),
);
