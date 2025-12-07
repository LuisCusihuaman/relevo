import { useMemo } from "react";
import { useUser } from "@clerk/clerk-react";
import { getInitials } from "@/lib/formatters";

export interface UserInfo {
	id: string;
	name: string;
	initials: string;
	role: string;
}

export function useCurrentPhysician(): UserInfo {
	const { user: clerkUser } = useUser();

	return useMemo(() => {
		if (!clerkUser) {
			return {
				id: "unknown",
				name: "Unknown User",
				initials: "U",
				role: "Unknown",
			};
		}

		const name =
			clerkUser.fullName ||
			`${clerkUser.firstName || ""} ${clerkUser.lastName || ""}`.trim();
		const roles = (clerkUser.publicMetadata?.['roles'] as Array<string>) || [];

		return {
			id: clerkUser.id,
			name: name,
			initials: getInitials(name),
			role: roles.join(", ") || "Doctor",
		};
	}, [clerkUser]);
}
