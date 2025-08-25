export function formatDiagnosis(input: unknown): string {
  if (typeof input === "string") return input;
  if (
    input &&
    typeof input === "object" &&
    "primary" in input &&
    typeof (input as { primary: unknown }).primary === "string"
  ) {
    const diagnosis = input as { primary: string; secondary?: string[] };
    const secondary = Array.isArray(diagnosis.secondary)
      ? ` — ${diagnosis.secondary.join(", ")}`
      : "";
    return `${diagnosis.primary}${secondary}`;
  }
  return "";
}


