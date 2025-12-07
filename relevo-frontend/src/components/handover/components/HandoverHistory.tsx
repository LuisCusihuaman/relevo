import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  ArrowRight,
  Calendar,
  Clock,
  Eye,
  FileText,
  History,
  User,
  X,
} from "lucide-react";
import { useState, type JSX } from "react";
import { useTranslation } from "react-i18next";
import { useHandoverActivityLog } from "@/api";

interface PatientData {
  name: string;
  mrn: string;
  admissionDate: string;
}

interface HandoverHistoryProps {
  onClose: () => void;
  patientData: PatientData;
  handoverId: string;
  hideHeader?: boolean;
}

export function HandoverHistory({
  onClose,
  patientData,
  handoverId,
  hideHeader = false,
}: HandoverHistoryProps): JSX.Element {
  const { t } = useTranslation("handoverHistory");
  const [selectedHandover, setSelectedHandover] = useState<string | null>(null);

  // Fetch handover activity log
  const { data: activityLog, isLoading, error } = useHandoverActivityLog(handoverId);

  // Transform activity log data into handover history format
  const handoverHistory = activityLog && activityLog.length > 0
    ? activityLog.slice(0, 10).map((activity) => ({
        id: activity.id,
        date: new Date(activity.createdAt).toLocaleDateString(),
        shift: "Current Shift", // Could be derived from activity metadata
        time: new Date(activity.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        status: "completed", // All past activities are completed
        outgoingTeam: "Previous Team", // Could be derived from activity
        incomingTeam: "Current Team", // Could be derived from activity
        primaryPhysician: activity.userName,
        receivingPhysician: "Current Physician", // Could be derived from context
        severity: "stable", // Could be derived from activity type
        keyPoints: [
          activity.activityDescription || activity.activityType,
        ].filter(Boolean),
      }))
    : [
        // Fallback data when no activity log is available
        {
          id: "current",
          date: new Date().toLocaleDateString(),
          shift: "shifts.dayToEvening",
          time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
          status: "in-progress",
          outgoingTeam: "teams.dayInternal",
          incomingTeam: "teams.eveningInternal",
          primaryPhysician: "Current Physician",
          receivingPhysician: "Next Physician",
          severity: "stable",
          keyPoints: [
            "keyPoints.currentHandover",
            "keyPoints.patientStable",
          ],
        },
      ];

  const getSeverityColor = (severity: string): string => {
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

  const getStatusColor = (status: string): string => {
    switch (status) {
      case "completed":
        return "bg-green-100 text-green-800";
      case "in-progress":
        return "bg-blue-100 text-blue-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="h-full flex flex-col bg-white">
        {!hideHeader && (
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">
              {t("handoverHistory")}
            </h2>
            <Button
              className="h-8 w-8 p-0"
              size="sm"
              variant="ghost"
              onClick={onClose}
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        )}
        <div className="flex-1 flex items-center justify-center">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">{t("loadingHistory")}</p>
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="h-full flex flex-col bg-white">
        {!hideHeader && (
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">
              {t("handoverHistory")}
            </h2>
            <Button
              className="h-8 w-8 p-0"
              size="sm"
              variant="ghost"
              onClick={onClose}
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        )}
        <div className="flex-1 flex items-center justify-center">
          <div className="text-center">
            <div className="text-red-500 mb-4">⚠️</div>
            <p className="text-red-600">{t("errorLoadingHistory")}</p>
            <p className="text-sm text-gray-500 mt-2">{error.message}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col bg-white">
      <div className="flex flex-col h-full">
        {/* Header - Only show if not hidden */}
        {!hideHeader && (
          <div className="flex items-center justify-between p-4 border-b border-gray-200">
            <h3 className="font-medium flex items-center space-x-2">
              <History className="w-4 h-4" />
              <span>{t("title")}</span>
            </h3>
            <Button size="sm" variant="ghost" onClick={onClose}>
              <X className="w-4 h-4" />
            </Button>
          </div>
        )}

        {/* Patient Context */}
        <div className="p-4 bg-blue-50 border-b border-gray-200">
          <h4 className="font-medium text-sm text-gray-900 mb-2">
            {t("patientTimeline")}
          </h4>
          <div className="text-sm text-gray-600">
            <p className="font-medium">{patientData.name}</p>
            <p>
              {t("mrn")} {patientData.mrn}
            </p>
            <p>
              {t("admitted")} {patientData.admissionDate}
            </p>
          </div>
        </div>

        <ScrollArea className="flex-1">
          <div className="p-4 space-y-4">
            {handoverHistory.map((handover) => (
              <Card
                key={handover.id}
                className={`cursor-pointer transition-all border ${
                  selectedHandover === handover.id
                    ? "border-blue-300 shadow-md"
                    : "border-gray-200 hover:border-gray-300"
                } ${handover.status === "in-progress" ? "ring-2 ring-blue-200" : ""}`}
                onClick={() =>
                  { setSelectedHandover(
                    selectedHandover === handover.id ? null : handover.id,
                  ); }
                }
              >
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <Calendar className="w-4 h-4 text-gray-500" />
                      <span className="text-sm font-medium">
                        {handover.date}
                      </span>
                    </div>
                    <Badge className={getStatusColor(handover.status)}>
                      {t(
                        handover.status === "in-progress"
                          ? "status.current"
                          : "status.completed",
                      )}
                    </Badge>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Clock className="w-3 h-3 text-gray-400" />
                    <span className="text-xs text-gray-600">
                      {handover.time}
                    </span>
                    <ArrowRight className="w-3 h-3 text-gray-400" />
                    <span className="text-xs text-gray-600">
                      {t(handover.shift)}
                    </span>
                  </div>
                </CardHeader>

                <CardContent className="pt-0">
                  <div className="space-y-3">
                    {/* Severity */}
                    <div className="flex items-center justify-between">
                      <span className="text-xs text-gray-600">
                        {t("severityLabel")}
                      </span>
                      <Badge className={getSeverityColor(handover.severity)}>
                        {t(`severity.${handover.severity}`).toUpperCase()}
                      </Badge>
                    </div>

                    {/* Team Transition */}
                    <div className="space-y-1">
                      <div className="flex items-center space-x-2 text-xs">
                        <User className="w-3 h-3 text-gray-400" />
                        <span className="text-gray-600">
                          {t("from")} {t(handover.primaryPhysician)}
                        </span>
                      </div>
                      <div className="flex items-center space-x-2 text-xs">
                        <ArrowRight className="w-3 h-3 text-gray-400" />
                        <span className="text-gray-600">
                          {t("to")} {t(handover.receivingPhysician)}
                        </span>
                      </div>
                    </div>

                    {/* Key Points Preview */}
                    <div className="space-y-1">
                      <span className="text-xs text-gray-600">
                        {t("keyPointsLabel")}
                      </span>
                      <ul className="text-xs text-gray-700 space-y-1">
                        {handover.keyPoints.slice(0, 2).map((point, index) => (
                          <li key={index} className="flex items-start space-x-1">
                            <div className="w-1 h-1 bg-gray-400 rounded-full mt-2 flex-shrink-0"></div>
                            <span>{t(point)}</span>
                          </li>
                        ))}
                        {handover.keyPoints.length > 2 && (
                          <li className="text-gray-500">
                            {t("moreKeyPoints", {
                              count: handover.keyPoints.length - 2,
                            })}
                          </li>
                        )}
                      </ul>
                    </div>

                    {/* Expanded Details */}
                    {selectedHandover === handover.id && (
                      <div className="pt-3 border-t border-gray-200 space-y-3">
                        <div>
                          <span className="text-xs text-gray-600 font-medium">
                            {t("teamsLabel")}
                          </span>
                          <div className="text-xs text-gray-700 mt-1">
                            <p>
                              {t("out")} {t(handover.outgoingTeam)}
                            </p>
                            <p>
                              {t("in")} {t(handover.incomingTeam)}
                            </p>
                          </div>
                        </div>

                        <div>
                          <span className="text-xs text-gray-600 font-medium">
                            {t("allKeyPointsLabel")}
                          </span>
                          <ul className="text-xs text-gray-700 space-y-1 mt-1">
                            {handover.keyPoints.map((point, index) => (
                              <li
                                key={index}
                                className="flex items-start space-x-1"
                              >
                                <div className="w-1 h-1 bg-gray-400 rounded-full mt-2 flex-shrink-0"></div>
                                <span>{t(point)}</span>
                              </li>
                            ))}
                          </ul>
                        </div>

                        <div className="flex space-x-2">
                          <Button
                            className="text-xs"
                            size="sm"
                            variant="outline"
                          >
                            <Eye className="w-3 h-3 mr-1" />
                            {t("viewFull")}
                          </Button>
                          <Button
                            className="text-xs"
                            size="sm"
                            variant="outline"
                          >
                            <FileText className="w-3 h-3 mr-1" />
                            {t("compare")}
                          </Button>
                        </div>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </ScrollArea>

        {/* Footer */}
        <div className="p-4 border-t border-gray-200">
          <div className="text-xs text-gray-500 text-center">
            {t("footer", { count: handoverHistory.length })}
          </div>
        </div>
      </div>
    </div>
  );
}
