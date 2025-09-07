import axios from "axios";

// Axios instance with default config
export const api = axios.create({
	baseURL: (import.meta.env["VITE_API_URL"] as string | undefined) || "https://localhost:57679",
	headers: {
		"Content-Type": "application/json",
		Accept: "application/json",
	},
});

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
	const token = localStorage.getItem("authToken");
	if (token) {
		config.headers.Authorization = `Bearer ${token}`;
	}
	return config;
});
