import type { useAuth } from "@clerk/clerk-react";
import { createRootRouteWithContext } from "@tanstack/react-router";

interface RootRouteContext {
  auth?: ReturnType<typeof useAuth>;
}

export const Route = createRootRouteWithContext<RootRouteContext>()({
  // Remove the redirect from root - let authenticated routes handle authentication
});