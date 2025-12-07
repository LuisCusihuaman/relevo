/**
 * API Mappers - Transform API types to domain types
 *
 * Rule: Concise-FP - Single responsibility, functional approach
 *
 * Usage:
 *   import { mapApiPatientHandoverData } from "@/api/mappers";
 *   const domainData = mapApiPatientHandoverData(apiResponse);
 */

export * from "./patient.mapper";
export * from "./handover.mapper";
export * from "./shift-check-in.mapper";
export * from "./user.mapper";
export * from "./ui.mapper";
