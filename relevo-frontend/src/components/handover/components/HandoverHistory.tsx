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
import { usePatientHandoverTimeline } from "@/api/endpoints/patients";
import { getSeverityBadgeColor, getStatusBadgeColor } from "@/lib/formatters";

interface PatientData {
  name: string;
  mrn: string;
  admissionDate: string;
}

interface HandoverHistoryProps {
  onClose: () => void;
  patientData: PatientData;
  patientId: string;
  currentHandoverId?: string;
  hideHeader?: boolean;
}

export function HandoverHistory({
  onClose,
  patientData,
  patientId,
  currentHandoverId,
  hideHeader = false,
}: HandoverHistoryProps): JSX.Element {
  const { t } = useTranslation("handoverHistory");
  const [selectedHandover, setSelectedHandover] = useState<string | null>(null);

  // Fetch handover timeline for the patient
  const { data: handoverData, isLoading, error } = usePatientHandoverTimeline(patientId);
  
  // Debug log - remove after testing
  console.log("[HandoverHistory] patientId:", patientId, "data:", handoverData, "isLoading:", isLoading, "error:", error);

  // Helper to get display name (avoid showing user IDs)
  const getDisplayName = (name: string | null | undefined, fallbackId?: string): string => {
    if (name && name.trim()) return name;
    // Don't show Clerk user IDs (start with "user_")
    if (fallbackId?.startsWith("user_")) return t("unknownUser");
    return fallbackId || t("unknownUser");
  };

  // Transform handover data into display format
  const handoverHistory = handoverData?.items?.map((handover) => ({
    id: handover.id,
    date: handover.createdAt 
      ? new Date(handover.createdAt).toLocaleDateString() 
      : new Date().toLocaleDateString(),
    shift: handover.shiftName || "shifts.dayToEvening",
    time: handover.createdAt 
      ? new Date(handover.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      : "",
    status: handover.id === currentHandoverId ? "in-progress" : handover.stateName.toLowerCase(),
    outgoingTeam: getDisplayName(handover.createdByName, handover.createdBy),
    incomingTeam: getDisplayName(handover.assignedToName, handover.assignedTo),
    primaryPhysician: getDisplayName(handover.responsiblePhysicianName),
    receivingPhysician: getDisplayName(handover.assignedToName, handover.assignedTo),
    severity: handover.illnessSeverity || "stable",
    completedAt: handover.completedAt,
  })) ?? [];

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
            {handoverHistory.length === 0 ? (
              <div className="text-center py-8">
                <History className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <p className="text-gray-500">{t("noHandovers")}</p>
              </div>
            ) : null}
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
                    <Badge className={getStatusBadgeColor(handover.status)}>
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
                      <Badge className={getSeverityBadgeColor(handover.severity)}>
                        {t(`severity.${handover.severity}`).toUpperCase()}
                      </Badge>
                    </div>

                    {/* Team Transition */}
                    <div className="space-y-1">
                      <div className="flex items-center space-x-2 text-xs">
                        <User className="w-3 h-3 text-gray-400" />
                        <span className="text-gray-600">
                          {t("from")} {handover.primaryPhysician}
                        </span>
                      </div>
                      <div className="flex items-center space-x-2 text-xs">
                        <ArrowRight className="w-3 h-3 text-gray-400" />
                        <span className="text-gray-600">
                          {t("to")} {handover.receivingPhysician}
                        </span>
                      </div>
                    </div>

                    {/* Completed info */}
                    {handover.completedAt && (
                      <div className="text-xs text-gray-500">
                        {t("completedAt", { 
                          date: new Date(handover.completedAt).toLocaleString() 
                        })}
                      </div>
                    )}

                    {/* Expanded Details */}
                    {selectedHandover === handover.id && (
                      <div className="pt-3 border-t border-gray-200">
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
