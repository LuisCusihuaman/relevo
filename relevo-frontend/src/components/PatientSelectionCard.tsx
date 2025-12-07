import { Activity, AlertTriangle, CheckCircle, Eye } from "lucide-react";
import type { ReactElement } from "react";
import type { ShiftCheckInPatient } from "@/types/domain";
import { getSeverityColor, getStatusColor } from "@/lib/formatters";

import i18n from "@/common/i18n";

interface PatientSelectionCardProps {
  patient: ShiftCheckInPatient;
  isSelected: boolean;
}

export function PatientSelectionCard({
  patient,
  isSelected,
}: PatientSelectionCardProps): ReactElement {
  const translate = (key: string, options?: Record<string, unknown>): string => {
    try {
      // Use i18n instance directly with patientSelectionCard namespace
      const result = i18n.t(key, { ns: 'patientSelectionCard', ...options });
      return result || key;
    } catch {
      return key;
    }
  };

  const getSeverityIcon = (severity: string): typeof Activity => {
    switch (severity) {
      case "unstable":
      case "Unstable":
        return AlertTriangle;
      case "watcher":
      case "Watcher":
        return Eye;
      case "stable":
      case "Stable":
        return CheckCircle;
      default:
        return Activity;
    }
  };

  const SeverityIcon = getSeverityIcon(patient.severity);

  return (
    <li
      className={`grid grid-cols-[minmax(0,2fr)_minmax(0,2fr)_minmax(0,1fr)] items-center gap-6 py-4 px-6 cursor-pointer transition-all border-2 rounded-lg ${
        isSelected ? 'border-primary bg-primary/5' : 'border-gray-100'
      }`}
    >
      {/* Patient Info Column */}
      <div className="flex items-center gap-3 min-w-0">
        <span className="h-10 w-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
          {patient.name?.charAt(0)?.toUpperCase() || '?'}
        </span>
        <div className="min-w-0 flex-1">
          <div className="text-sm font-medium text-gray-900 truncate">
            {patient.name || 'Unknown Patient'}
          </div>
          <div className="text-xs text-gray-600 truncate">
            {patient.age ? translate("age", { age: patient.age }) : translate("ageNotAvailable")} • {patient.room || 'Unknown Room'}
          </div>
        </div>
      </div>

      {/* Diagnosis Column */}
      <div className="min-w-0 flex-1">
        <div className="text-sm font-medium text-blue-600 truncate">
          {patient.diagnosis || 'No diagnosis'}
        </div>
        <div className="mt-1 flex items-center gap-2">
          <SeverityIcon className={`w-3.5 h-3.5 ${getSeverityColor(patient.severity)}`} />
          <span className="text-xs text-gray-600">
            {translate(`severity.${patient.severity}`)}
          </span>
        </div>
      </div>

      {/* Status and Actions Column */}
      <div className="flex items-center justify-end gap-2 shrink-0 min-w-0">
        <span className={`text-xs font-medium ${getStatusColor(patient.status)}`}>
          {patient.status === "pending" && translate("status.pending")}
          {patient.status === "in-progress" && translate("status.inProgress")}
          {patient.status === "complete" && translate("status.complete")}
        </span>
      </div>
    </li>
  );
}