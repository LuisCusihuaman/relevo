import type { TFunction } from "i18next";
import type { Collaborator } from "./types";

// REMOVED: Mock current user and patient data - now fetched from API
// Use real API data instead of hardcoded constants

// I-PASS Guidelines for each section
export const getIpassGuidelines = (t: TFunction<"handover", undefined>) => ({
  illness: {
    title: t("ipassGuidelines.illness.title"),
    points: [
      t("ipassGuidelines.illness.points.0"),
      t("ipassGuidelines.illness.points.1"),
      t("ipassGuidelines.illness.points.2"),
      t("ipassGuidelines.illness.points.3"),
    ],
  },
  patient: {
    title: t("ipassGuidelines.patient.title"),
    points: [
      t("ipassGuidelines.patient.points.0"),
      t("ipassGuidelines.patient.points.1"),
      t("ipassGuidelines.patient.points.2"),
      t("ipassGuidelines.patient.points.3"),
    ],
  },
  actions: {
    title: t("ipassGuidelines.actions.title"),
    points: [
      t("ipassGuidelines.actions.points.0"),
      t("ipassGuidelines.actions.points.1"),
      t("ipassGuidelines.actions.points.2"),
      t("ipassGuidelines.actions.points.3"),
    ],
  },
  awareness: {
    title: t("ipassGuidelines.awareness.title"),
    points: [
      t("ipassGuidelines.awareness.points.0"),
      t("ipassGuidelines.awareness.points.1"),
      t("ipassGuidelines.awareness.points.2"),
      t("ipassGuidelines.awareness.points.3"),
    ],
  },
  synthesis: {
    title: t("ipassGuidelines.synthesis.title"),
    points: [
      t("ipassGuidelines.synthesis.points.0"),
      t("ipassGuidelines.synthesis.points.1"),
      t("ipassGuidelines.synthesis.points.2"),
      t("ipassGuidelines.synthesis.points.3"),
    ],
  },
});

// CURRENT SHIFT HANDOVER PARTICIPANTS - Who is present RIGHT NOW
export const currentlyPresent: Collaborator[] = [
  // Currently assigned physician
  {
    id: 1,
    name: "Dr. Johnson",
    initials: "DJ",
    color: "bg-blue-600",
    status: "active",
    lastSeen: "now",
    activity: "Completing handover documentation",
    role: "Day Attending",
    presenceType: "assigned-current",
  },
  // Receiving physician
  {
    id: 2,
    name: "Dr. Patel",
    initials: "SP",
    color: "bg-purple-600",
    status: "active",
    lastSeen: "30s ago",
    activity: "Reviewing handover content",
    role: "Evening Attending",
    presenceType: "assigned-receiving",
  },
  // Other doctors present in current handover session
  {
    id: 3,
    name: "Dr. Martinez",
    initials: "AM",
    color: "bg-gray-600",
    status: "active",
    lastSeen: "1 min ago",
    activity: "Participating in handover session",
    role: "Day Attending",
    presenceType: "participating",
  },
  {
    id: 4,
    name: "Dr. Chen",
    initials: "LC",
    color: "bg-gray-700",
    status: "active",
    lastSeen: "2 min ago",
    activity: "Joining evening shift",
    role: "Evening Attending",
    presenceType: "participating",
  },
  // Residents and support staff currently present
  {
    id: 5,
    name: "Dr. Rodriguez",
    initials: "MR",
    color: "bg-emerald-600",
    status: "active",
    lastSeen: "1 min ago",
    activity: "Taking notes",
    role: "Resident",
    presenceType: "supporting",
  },
  {
    id: 6,
    name: "Nurse Clara",
    initials: "CJ",
    color: "bg-teal-600",
    status: "viewing",
    lastSeen: "3 min ago",
    activity: "Reviewing care plan",
    role: "Charge Nurse",
    presenceType: "supporting",
  },
  {
    id: 7,
    name: "Dr. Wilson",
    initials: "RW",
    color: "bg-gray-500",
    status: "viewing",
    lastSeen: "5 min ago",
    activity: "Observing handover",
    role: "Day Attending",
    presenceType: "participating",
  },
];

// Filter active participants for main interface
export const activeCollaborators = currentlyPresent.filter(
  (p) => p.status === "active" || p.status === "viewing",
);

// Recent activity feed
export const recentActivity = [
  {
    id: 1,
    user: "Dr. Patel",
    userInitials: "SP",
    userColor: "bg-purple-600",
    action: "joined handover session",
    section: "General",
    time: "30 seconds ago",
    type: "user_joined",
  },
  {
    id: 2,
    user: "Dr. Rodriguez",
    userInitials: "MR",
    userColor: "bg-emerald-600",
    action: "added note about respiratory status",
    section: "Action List",
    time: "1 minute ago",
    type: "content_added",
  },
  {
    id: 3,
    user: "Dr. Johnson",
    userInitials: "DJ",
    userColor: "bg-blue-600",
    action: "updated patient summary",
    section: "Patient Summary",
    time: "2 minutes ago",
    type: "content_updated",
  },
  {
    id: 4,
    user: "Dr. Chen",
    userInitials: "LC",
    userColor: "bg-gray-700",
    action: "joined handover session",
    section: "General",
    time: "2 minutes ago",
    type: "user_joined",
  },
];
