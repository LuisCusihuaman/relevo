import { useAuth } from "@clerk/clerk-react";
import { createAuthenticatedApiCall } from "@/api/client";
import { useMemo } from "react";

/**
 * Hook to create authenticated API calls using Clerk tokens
 */
export const useAuthenticatedApi = (): {
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>;
} => {
	// Session token from Clerk
	const { getToken } = useAuth();

	const authenticatedApiCall = useMemo(
		() => createAuthenticatedApiCall(getToken),
		[getToken]
	);

	return {
		authenticatedApiCall,
	};
};
