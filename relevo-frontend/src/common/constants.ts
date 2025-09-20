import type { User, PatientData, Collaborator } from "./types";

// Mock current user data
export const currentUser: User = {
  name: "Dr. Sarah Johnson",
  role: "Attending Physician",
  shift: "Day Shift",
  initials: "SJ",
};

// Mock patient data
export const patientData: PatientData = {
  id: "P001",
  name: "John Smith",
  age: 45,
  mrn: "MRN12345",
  admissionDate: "2024-01-15T08:30:00Z",
  currentDateTime: "2024-01-16T14:30:00Z",
  primaryTeam: "Cardiology",
  primaryDiagnosis: "Acute Myocardial Infarction",
  severity: "High",
  handoverStatus: "In Progress",
  shift: "Day",
  room: "ICU-201",
  unit: "Intensive Care Unit",
  handoverTime: "17:00",
  assignedPhysician: {
    name: "Dr. Sarah Johnson",
    role: "Attending Physician",
    initials: "SJ",
    color: "#3B82F6",
    shiftEnd: "17:00",
    shiftStart: "07:00",
    status: "active",
    patientAssignment: "Primary",
  },
  receivingPhysician: {
    name: "Dr. Michael Chen",
    role: "Night Shift Attending",
    initials: "MC",
    color: "#10B981",
    shiftEnd: "07:00",
    shiftStart: "17:00",
    status: "scheduled",
    patientAssignment: "Receiving",
  },
};

// Mock collaborators data
export const activeCollaborators: Array<Collaborator> = [
  {
    id: 1,
    name: "Dr. Sarah Johnson",
    initials: "SJ",
    color: "#3B82F6",
    status: "active",
    lastSeen: "2024-01-16T14:30:00Z",
    activity: "Documenting patient summary",
    role: "Attending Physician",
    presenceType: "assigned-current",
  },
  {
    id: 2,
    name: "Dr. Michael Chen",
    initials: "MC",
    color: "#10B981",
    status: "viewing",
    lastSeen: "2024-01-16T14:25:00Z",
    activity: "Reviewing handover notes",
    role: "Night Shift Attending",
    presenceType: "assigned-receiving",
  },
  {
    id: 3,
    name: "Nurse Emma Wilson",
    initials: "EW",
    color: "#F59E0B",
    status: "active",
    lastSeen: "2024-01-16T14:28:00Z",
    activity: "Updating vital signs",
    role: "ICU Nurse",
    presenceType: "participating",
  },
];

// Helper function to get currently present collaborators
export const currentlyPresent = activeCollaborators.filter(
  (user) => user.status === "active" || user.status === "viewing"
);
