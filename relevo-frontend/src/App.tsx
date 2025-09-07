import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { RouterProvider } from "@tanstack/react-router";
import type { FunctionComponent } from "./common/types";
import type { TanstackRouter } from "./main";
import { TanStackRouterDevelopmentTools } from "./components/utils/development-tools/TanStackRouterDevelopmentTools";
import { useSyncClerkUser } from "./store/user.store";

const queryClient = new QueryClient({
	defaultOptions: {
		queries: {
			refetchOnWindowFocus: false, // Prevent refetch when switching tabs globally
			staleTime: 5 * 60 * 1000, // 5 minutes
			gcTime: 15 * 60 * 1000, // 15 minutes
		},
	},
});

type AppProps = { router: TanstackRouter };

const App = ({ router }: AppProps): FunctionComponent => {
	// Sync Clerk user data with our user store
	useSyncClerkUser();

	return (
		<QueryClientProvider client={queryClient}>
			<RouterProvider router={router} />
			<TanStackRouterDevelopmentTools
				initialIsOpen={false}
				position="bottom-left"
				router={router}
			/>
			<ReactQueryDevtools initialIsOpen={false} position="bottom" />
		</QueryClientProvider>
	);
};

export default App;
