import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Activity, Calendar, Clock, MapPin, User, Users } from "lucide-react";
import { useTranslation } from "react-i18next";

interface PatientHeaderProps {
  patient: {
    id: string;
    name: string;
    age: number;
    mrn: string;
    admissionDate: string;
    currentDateTime: string;
    primaryTeam: string;
    severity: string;
    handoverStatus: string;
    shift: string;
    room: string;
    primaryDiagnosis: string;
  };
}

export function PatientHeader({ patient }: PatientHeaderProps) {
  const { t } = useTranslation("patientHeader");

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case "stable":
        return "bg-green-100 text-green-800 border-green-200";
      case "guarded":
        return "bg-yellow-100 text-yellow-800 border-yellow-200";
      case "unstable":
        return "bg-orange-100 text-orange-800 border-orange-200";
      case "critical":
        return "bg-red-100 text-red-800 border-red-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "in-progress":
        return "bg-blue-100 text-blue-800";
      case "completed":
        return "bg-green-100 text-green-800";
      case "pending":
        return "bg-yellow-100 text-yellow-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const losDays = Math.ceil(
    (new Date().getTime() - new Date(patient.admissionDate).getTime()) /
      (1000 * 3600 * 24),
  );

  return (
    <Card className="mx-6 mt-6 border-l-4 border-l-blue-500">
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          {/* Patient Information */}
          <div className="flex items-center space-x-6">
            <Avatar className="w-16 h-16">
              <AvatarFallback className="bg-blue-100 text-blue-600 text-lg">
                {patient.name
                  .split(" ")
                  .map((n) => n[0])
                  .join("")}
              </AvatarFallback>
            </Avatar>

            <div className="space-y-2">
              <div className="flex items-center space-x-4">
                <h2 className="text-2xl font-bold text-gray-900">
                  {patient.name}
                </h2>
                <Badge className={getSeverityColor(patient.severity)}>
                  {t(`severity.${patient.severity}`).toUpperCase()}
                </Badge>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm text-gray-600">
                <div className="flex items-center space-x-1">
                  <User className="w-4 h-4" />
                  <span>{t("age", { age: patient.age })}</span>
                </div>
                <div className="flex items-center space-x-1">
                  <MapPin className="w-4 h-4" />
                  <span>{t("room", { room: patient.room })}</span>
                </div>
                <div className="flex items-center space-x-1">
                  <Calendar className="w-4 h-4" />
                  <span>{t("admitted", { date: patient.admissionDate })}</span>
                </div>
                <div className="flex items-center space-x-1">
                  <Users className="w-4 h-4" />
                  <span>{patient.primaryTeam}</span>
                </div>
                <div className="flex items-center space-x-1">
                  <Activity className="w-4 h-4" />
                  <span>
                    {patient.primaryDiagnosis.includes('.') 
                      ? t(patient.primaryDiagnosis) 
                      : patient.primaryDiagnosis}
                  </span>
                </div>
              </div>

              <div className="text-xs text-gray-500">
                {t("mrn", { mrn: patient.mrn })}
              </div>
            </div>
          </div>

          {/* Handover Status */}
          <div className="text-right space-y-2">
            <div className="flex items-center justify-end space-x-2">
              <Clock className="w-4 h-4 text-gray-500" />
              <span className="text-sm text-gray-600">
                {patient.currentDateTime}
              </span>
            </div>

            <div className="space-y-1">
              <Badge className={getStatusColor(patient.handoverStatus)}>
                {t(
                  patient.handoverStatus === "in-progress"
                    ? "handoverInProgress"
                    : "handoverComplete",
                )}
              </Badge>
              <div className="text-xs text-gray-500">{patient.shift}</div>
            </div>

            {/* Length of Stay */}
            <div className="text-xs text-gray-500">
              {t("los", { count: losDays })}
            </div>
          </div>
        </div>

        {/* Active Handover Indicator */}
        {patient.handoverStatus === "in-progress" && (
          <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex items-center space-x-2">
              <Activity className="w-4 h-4 text-blue-600" />
              <span className="text-sm font-medium text-blue-800">
                {t("activeHandoverSession", { shift: patient.shift })}
              </span>
              <div className="w-2 h-2 bg-blue-600 rounded-full animate-pulse"></div>
            </div>
            <p className="text-xs text-blue-700 mt-1">
              {t("collaborationMessage")}
            </p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
