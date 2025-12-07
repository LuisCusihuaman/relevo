/**
 * Helper types for extracting OpenAPI schema types
 * Rule: Strict-TS - Explicit return types, prefer type over interface
 */
import type { components, operations } from "./schema";

// =============================================================================
// Schema Components - Direct access to API DTOs
// =============================================================================
export type Schemas = components["schemas"];

// =============================================================================
// Operation Helpers - Extract request/response types from operations
// =============================================================================

/** Extract the success response body type from an operation */
export type ResponseBody<T extends keyof operations> =
	operations[T] extends { responses: { 200: { content: { "application/json": infer R } } } }
		? R
		: never;

/** Extract the request body type from an operation */
export type RequestBody<T extends keyof operations> =
	operations[T] extends { requestBody: { content: { "application/json": infer R } } }
		? R
		: never;

/** Extract path parameters from an operation */
export type PathParams<T extends keyof operations> =
	operations[T] extends { parameters: { path: infer P } } ? P : never;

/** Extract query parameters from an operation */
export type QueryParams<T extends keyof operations> =
	operations[T] extends { parameters: { query: infer Q } } ? Q : never;

// =============================================================================
// Common API Types - Re-exported for convenience
// =============================================================================

// Patient types
export type ApiPatientRecord = Schemas["PatientRecord"];
export type ApiPatientSummaryCard = Schemas["PatientSummaryCard"];
export type ApiPatientSummaryDto = Schemas["PatientSummaryDto"];
export type ApiGetPatientByIdResponse = Schemas["GetPatientByIdResponse"];
export type ApiGetPatientHandoverDataResponse = Schemas["GetPatientHandoverDataResponse"];
export type ApiPhysicianDto = Schemas["GetPatientHandoverDataResponse_PhysicianDto"];

// Handover types
export type ApiHandoverRecord = Schemas["HandoverRecord"];
export type ApiHandoverDto = Schemas["HandoverDto"];
export type ApiGetHandoverByIdResponse = Schemas["GetHandoverByIdResponse"];
export type ApiHandoverActionItemFullRecord = Schemas["HandoverActionItemFullRecord"];
export type ApiContingencyPlanRecord = Schemas["ContingencyPlanRecord"];
export type ApiContingencyPlanDto = Schemas["ContingencyPlanDto"];
export type ApiSituationAwarenessDto = Schemas["SituationAwarenessDto"];
export type ApiSynthesisDto = Schemas["SynthesisDto"];
export type ApiHandoverMessageRecord = Schemas["HandoverMessageRecord"];

// Clinical data types
export type ApiGetClinicalDataResponse = Schemas["GetClinicalDataResponse"];
export type ApiUpdateClinicalDataRequest = Schemas["UpdateClinicalDataRequest"];

// Shift Check-In types
export type ApiShiftRecord = Schemas["ShiftRecord"];
export type ApiUnitRecord = Schemas["UnitRecord"];
export type ApiGetPatientsByUnitResponse = Schemas["GetPatientsByUnitResponse"];

// User types
export type ApiGetMyProfileResponse = Schemas["GetMyProfileResponse"];

// Pagination types
export type ApiPaginationInfo = Schemas["PaginationInfo"];

// Request types
export type ApiCreateContingencyPlanRequest = Schemas["CreateContingencyPlanRequest"];
export type ApiUpdateSituationAwarenessRequest = Schemas["UpdateSituationAwarenessRequest"];
export type ApiPutSynthesisRequest = Schemas["PutSynthesisRequest"];
export type ApiCreateHandoverActionItemRequest = Schemas["CreateHandoverActionItemRequest"];
export type ApiPostAssignmentsRequest = Schemas["PostAssignmentsRequest"];
