import { Activity, AlertTriangle, CheckCircle, Eye } from "lucide-react";
import type { ReactElement } from "react";
import type { SetupPatient } from "@/common/types";

interface PatientSelectionCardProps {
  patient: SetupPatient;
  isSelected: boolean;
  translation?: (key: string, options?: Record<string, unknown>) => string;
}

export function PatientSelectionCard({
  patient,
  isSelected,
  translation: t = ((key: string) => key) as (key: string, options?: Record<string, unknown>) => string,
}: PatientSelectionCardProps): ReactElement {
  const translate = (key: string): string => {
    try {
      return String(t(key));
    } catch {
      return key;
    }
  };

  const getSeverityIcon = (severity: string): typeof Activity => {
    switch (severity) {
      case "unstable":
        return AlertTriangle;
      case "watcher":
        return Eye;
      case "stable":
        return CheckCircle;
      default:
        return Activity;
    }
  };

  const getSeverityColor = (severity: string): string => {
    switch (severity) {
      case "unstable":
        return "text-red-600";
      case "watcher":
        return "text-yellow-600";
      case "stable":
        return "text-green-600";
      default:
        return "text-gray-600";
    }
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case "pending":
        return "text-orange-600";
      case "in-progress":
        return "text-blue-600";
      case "complete":
        return "text-green-600";
      default:
        return "text-gray-600";
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
            {patient.age ? `${patient.age} ${translate("ageUnit")}` : translate("ageNotAvailable")} • {patient.room || 'Unknown Room'}
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
        <div className="text-xs text-gray-500">
          {isSelected
            ? translate("toggleAssignment.remove")
            : translate("toggleAssignment.add")}
        </div>
      </div>
    </li>
  );
}