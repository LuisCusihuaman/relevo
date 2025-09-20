import { useCurrentUserProfile } from "@/api";
import { useMemo } from "react";

export interface CurrentUserData {
	name: string;
	role: string;
	shift: string;
	initials: string;
}

export function useCurrentUser(): {
	user: CurrentUserData | null;
	isLoading: boolean;
	error: unknown;
} {
	const { data: userProfile, isLoading, error } = useCurrentUserProfile();

	const user = useMemo((): CurrentUserData | null => {
		if (!userProfile) return null;
		if (error) return null;

		// Transform API user data to match the expected format
		const role = (userProfile.roles && userProfile.roles.length > 0)
			? userProfile.roles[0] // Use first role as primary role
			: "Physician"; // Default fallback

		const shift = "Current Shift"; // This could be enhanced to get from API later

		// Generate initials from first and last name
		const firstName = userProfile.firstName || "";
		const lastName = userProfile.lastName || "";
		const firstInitial = firstName.charAt(0).toUpperCase();
		const lastInitial = lastName.charAt(0).toUpperCase();
		const initials = `${firstInitial}${lastInitial}`;

		const fullName = userProfile.fullName || `${firstName} ${lastName}`;

		return {
			name: fullName,
			role: role || "Physician",
			shift,
			initials,
		};
	}, [userProfile, error]);

	return {
		user,
		isLoading,
		error,
	};
}
