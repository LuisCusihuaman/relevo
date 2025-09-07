import React from "react";
import { create } from "zustand";
import { useAuth, useUser } from "@clerk/clerk-react";

export interface ClerkUser {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	fullName: string;
	imageUrl?: string;
	roles: Array<string>;
	permissions: Array<string>;
}

type UserState = {
	// Clerk user data
	clerkUser: ClerkUser | null;
	isAuthenticated: boolean;

	// Legacy fields (for backward compatibility)
	doctorName: string;
	unitName: string;

	// Actions
	setClerkUser: (user: ClerkUser | null) => void;
	setAuthenticated: (authenticated: boolean) => void;

	// Legacy actions
	setDoctorName: (name: string) => void;
	setUnitName: (unit: string) => void;

	// Computed properties
	get fullName(): string;
	get email(): string;
	get isClinician(): boolean;
};

export const useUserStore = create<UserState>((set, get) => ({
	clerkUser: null,
	isAuthenticated: false,
	doctorName: "",
	unitName: "",

	setClerkUser: (user: ClerkUser | null): void => {
		set({ clerkUser: user, isAuthenticated: !!user });
	},

	setAuthenticated: (authenticated: boolean): void => {
		set({ isAuthenticated: authenticated });
	},

	setDoctorName: (name: string): void => {
		set({ doctorName: name });
	},

	setUnitName: (unit: string): void => {
		set({ unitName: unit });
	},

	get fullName(): string {
		const state = get();
		if (state.clerkUser) {
			return state.clerkUser.fullName;
		}
		return state.doctorName || "Unknown User";
	},

	get email(): string {
		const state = get();
		return state.clerkUser?.email || "";
	},

	get isClinician(): boolean {
		const state = get();
		return state.clerkUser?.roles.includes("clinician") ?? false;
	},
}));

/**
 * Hook to sync Clerk user data with the user store
 */
export const useSyncClerkUser = (): void => {
	const { user } = useUser();
	const { isLoaded } = useAuth();
	const { setClerkUser, setAuthenticated } = useUserStore();

	React.useEffect(() => {
		if (isLoaded && user) {
			const clerkUser: ClerkUser = {
				id: user.id,
				email: user.primaryEmailAddress?.emailAddress || "",
				firstName: user.firstName || "",
				lastName: user.lastName || "",
				fullName: user.fullName || "",
				imageUrl: user.imageUrl,
				roles: ["clinician"], // Default role, can be customized based on Clerk metadata
				permissions: [
					"patients.read",
					"patients.assign",
					"handovers.read",
					"handovers.create",
				],
			};

			setClerkUser(clerkUser);
			setAuthenticated(true);
		} else if (isLoaded && !user) {
			setClerkUser(null);
			setAuthenticated(false);
		}
	}, [user, isLoaded, setClerkUser, setAuthenticated]);
};
