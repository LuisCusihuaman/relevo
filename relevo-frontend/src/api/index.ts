// Re-export everything for backward compatibility
export * from "./client";
export type * from "./types";
export * from "./endpoints/patients";
export * from "./endpoints/handovers";
export * from "./endpoints/setup";
export * from "./mappers";

// Legacy exports for backward compatibility
export { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";