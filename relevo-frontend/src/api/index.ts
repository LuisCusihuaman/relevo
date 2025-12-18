// Re-export everything for backward compatibility
export * from "./client";
export * from "./endpoints/patients";
export * from "./endpoints/handovers";
export * from "./endpoints/shift-check-in";
export * from "./endpoints/users";

// Mappers - all mappers from the mappers directory
export * from "./mappers/index";

// Re-export generated types for API consumers
export type * from "./generated";
