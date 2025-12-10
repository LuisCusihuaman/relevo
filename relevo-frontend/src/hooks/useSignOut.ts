import { useCallback } from "react";
import { useClerk } from "@clerk/clerk-react";
import { useShiftCheckInStore } from "@/store/shift-check-in.store";

type UseSignOutReturn = {
	signOut: () => Promise<void>;
};

/**
 * Custom hook for signing out that clears all user-specific state.
 * This ensures shift check-in state is reset between different users.
 * 
 * Rule: Concise-FP - Functional hook with single responsibility
 */
export function useSignOut(): UseSignOutReturn {
	const { signOut: clerkSignOut } = useClerk();
	const { reset: resetShiftCheckIn } = useShiftCheckInStore();

	const signOut = useCallback(async (): Promise<void> => {
		// Clear shift check-in completion flag
		window.localStorage.removeItem("dailySetupCompleted");
		
		// Reset Zustand store (this also clears persisted state)
		resetShiftCheckIn();
		
		// Sign out from Clerk
		await clerkSignOut();
	}, [clerkSignOut, resetShiftCheckIn]);

	return { signOut };
}
