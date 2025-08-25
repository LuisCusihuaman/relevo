import type { SetupPatient } from "@/common/types";

export const dailySetupPatients: Array<SetupPatient> = [
  {
    id: 1,
    name: "Ava Thompson",
    age: 7,
    room: "201A",
    diagnosis: "Asthma exacerbation",
    status: "pending",
    severity: "watcher",
  },
  {
    id: 2,
    name: "Liam Rodriguez",
    age: 4,
    room: "202B",
    diagnosis: "Pneumonia",
    status: "in-progress",
    severity: "unstable",
  },
  {
    id: 3,
    name: "Mia Patel",
    age: 9,
    room: "203C",
    diagnosis: "Post-op appendectomy",
    status: "complete",
    severity: "stable",
  },
];


