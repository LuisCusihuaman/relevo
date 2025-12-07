/**
 * User mappers - Transform API types to domain types
 * Rule: Concise-FP - Functional, no classes
 */
import type { ApiGetMyProfileResponse } from "@/api/generated";
import type { User } from "@/types/domain";

export function mapApiUserProfile(api: ApiGetMyProfileResponse): User {
	return {
		id: api.id ?? "",
		email: api.email ?? "",
		firstName: api.firstName ?? "",
		lastName: api.lastName ?? "",
		fullName: api.fullName ?? "",
		roles: api.roles ?? [],
		isActive: api.isActive ?? false,
	};
}
