import type { PatientHandoverData } from "@/api";

export interface UserInfo {
  id: string;
  name: string;
  initials: string;
  role: string;
}

export const getInitials = (name?: string | null): string => {
  if (!name) return "U";
  return name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase();
};

export const toPhysician = (
  user: {
    id: string;
    firstName?: string | null;
    lastName?: string | null;
    fullName?: string | null;
    publicMetadata?: { roles?: unknown };
  } | null | undefined
): UserInfo => {
  if (!user) {
    return {
      id: "unknown",
      name: "Unknown User",
      initials: "U",
      role: "Unknown",
    };
  }

  const roles = Array.isArray(user.publicMetadata?.roles)
    ? (user.publicMetadata?.roles as Array<string>)
    : [];

  const name = user.fullName ?? `${user.firstName} ${user.lastName}`;

  return {
    id: user.id,
    name: name,
    initials: getInitials(name),
    role: roles.join(", ") || "Doctor",
  };
};

export const formatPhysician = (
  physician:
    | PatientHandoverData["assignedPhysician"]
    | PatientHandoverData["receivingPhysician"]
    | null
    | undefined
): { name: string; initials: string; role: string } => {
  if (!physician) {
    return {
      name: "Unknown",
      initials: "U",
      role: "Doctor",
    };
  }
  return {
    name: physician.name,
    initials: getInitials(physician.name),
    role: physician.role || "Doctor",
  };
};
