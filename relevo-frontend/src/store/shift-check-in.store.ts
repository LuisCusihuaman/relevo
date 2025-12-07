import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { ShiftCheckInState } from "@/features/shift-check-in";

type ShiftCheckInStoreState = Pick<ShiftCheckInState, "currentStep" | "unit" | "shift" | "selectedIndexes">;

type ShiftCheckInStoreActions = {
	setState: (newState: Partial<ShiftCheckInStoreState>) => void;
	reset: () => void;
};

const initialState: ShiftCheckInStoreState = {
	currentStep: 0,
	unit: "",
	shift: "",
	selectedIndexes: [],
};

export const useShiftCheckInStore = create<ShiftCheckInStoreState & ShiftCheckInStoreActions>()(
	persist(
		(set) => ({
			...initialState,
			setState: (newState: Partial<ShiftCheckInStoreState>): void => { set((state) => ({ ...state, ...newState })); },
			reset: (): void => { set(initialState); },
		}),
		{
			name: "shift-check-in-storage", // unique name for localStorage key
		},
	),
);
