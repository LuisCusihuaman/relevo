import type { useAuth } from "@clerk/clerk-react";
import {
  Outlet,
  createRootRouteWithContext,
} from "@tanstack/react-router";

interface RootRouteContext {
  auth?: ReturnType<typeof useAuth>;
}

export const Route = createRootRouteWithContext<RootRouteContext>()({
  component: 
    Outlet,
});