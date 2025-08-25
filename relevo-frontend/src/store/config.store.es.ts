import type { ShiftConfig, UnitConfig } from "@/common/types";

export const unitsConfigES: Array<UnitConfig> = [
  { id: "picu", name: "UCI Pediátrica", description: "Cuidados críticos pediátricos" },
  { id: "nicu", name: "UCI Neonatal", description: "Cuidados para recién nacidos" },
  { id: "general", name: "Pediatría General", description: "Unidad pediátrica general" },
  { id: "cardiology", name: "Cardiología", description: "Unidad de cardiología" },
  { id: "surgery", name: "Cirugía", description: "Cuidados postoperatorios" },
];

export const shiftsConfigES: Array<ShiftConfig> = [
  { id: "morning", name: "Mañana", time: "07:00 - 15:00" },
  { id: "evening", name: "Tarde", time: "15:00 - 23:00" },
  { id: "night", name: "Noche", time: "23:00 - 07:00" },
];


