import type { ShiftConfig, UnitConfig } from "@/common/types";

export const unitsConfig: Array<UnitConfig> = [
  {
    id: "picu",
    name: "Pediatric ICU",
    description: "Critical care for pediatric patients",
  },
  { id: "nicu", name: "Neonatal ICU", description: "Care for newborns" },
  {
    id: "general",
    name: "General Pediatrics",
    description: "General inpatient pediatric unit",
  },
  { id: "cardiology", name: "Cardiology", description: "Heart care unit" },
  { id: "surgery", name: "Surgery", description: "Post-op surgical care" },
];

export const shiftsConfig: Array<ShiftConfig> = [
  { id: "morning", name: "Morning", time: "07:00 - 15:00" },
  { id: "evening", name: "Evening", time: "15:00 - 23:00" },
  { id: "night", name: "Night", time: "23:00 - 07:00" },
];


