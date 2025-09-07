import axios from "axios";

// Axios instance with default config
export const api = axios.create({
	baseURL: (import.meta.env["VITE_API_URL"] as string | undefined) || "https://localhost:57679",
	headers: {
		"Content-Type": "application/json",
		Accept: "application/json",
	},
});

// Helper function to create authenticated API calls
export const createAuthenticatedApiCall = (getToken: () => Promise<string | null>) => {
	return async <T = unknown>(config: Parameters<typeof api.request>[0]): Promise<T> => {
		try {
			const token = await getToken();

			if (token) {
				// Use Clerk's preferred header format that matches our backend middleware
				config.headers = {
					...config.headers,
					"x-clerk-user-token": token,
					Authorization: `Bearer ${token}`,
				} as Record<string, string>;
			}

			const response = await api.request<T>(config);
			return response.data;
		} catch (error) {
			console.error("API call failed:", error);
			throw error;
		}
	};
};
