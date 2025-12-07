import axios from "axios";

// Token provider strategy
type TokenProvider = () => Promise<string | null>;
let tokenProvider: TokenProvider | null = null;

export function setTokenProvider(provider: TokenProvider | null): void {
	tokenProvider = provider;
}

// Axios instance with default config
export const api = axios.create({
	baseURL: (import.meta.env["VITE_API_URL"] as string | undefined) || "https://localhost:57679",
	headers: {
		"Content-Type": "application/json",
		Accept: "application/json",
	},
});

// Global Interceptor to inject Auth Token
api.interceptors.request.use(
	async (config) => {
		if (tokenProvider) {
			try {
				const token = await tokenProvider();
				if (token) {
					config.headers.Authorization = `Bearer ${token}`;
					config.headers["x-clerk-user-token"] = token;
				}
			} catch (error) {
				console.error("Error getting token in interceptor:", error);
				// We don't block the request here, we let the backend handle the 401 if needed
				// or maybe we should reject? For now, proceed without token if fetching fails.
			}
		}
		return config;
	},
	(error: unknown) => {
		return Promise.reject(error instanceof Error ? error : new Error(String(error)));
	}
);
