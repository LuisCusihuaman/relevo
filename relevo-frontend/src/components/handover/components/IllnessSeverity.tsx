import { useEffect, useState, type JSX } from "react";
import { Badge } from "@/components/ui/badge";
import {
  Brain,
  CheckCircle2,
  Clock,
  Edit,
  Eye,
  Heart,
  Stethoscope,
  Thermometer,
  User,
  Wifi,
} from "lucide-react";

import { useTranslation } from "react-i18next";

const severityLevelIds = ["stable", "guarded", "unstable", "critical"];
const severityIcons = {
  stable: Thermometer,
  guarded: Heart,
  unstable: Brain,
  critical: Stethoscope,
};
const severityStyling = {
  stable: {
    textColor: "text-emerald-600",
    bgColor: "bg-emerald-50",
    borderColor: "border-emerald-300",
  },
  guarded: {
    textColor: "text-amber-600",
    bgColor: "bg-amber-50",
    borderColor: "border-amber-300",
  },
  unstable: {
    textColor: "text-red-600",
    bgColor: "bg-red-50",
    borderColor: "border-red-300",
  },
  critical: {
    textColor: "text-red-700",
    bgColor: "bg-red-50",
    borderColor: "border-red-400",
  },
};

interface IllnessSeverityProps {
  currentUser: {
    name: string;
    initials: string;
    role: string;
  };
  assignedPhysician?: {
    name: string;
    initials: string;
    role: string;
  };
  severityContent?: string | null;
}

export function IllnessSeverity({
  currentUser,
  assignedPhysician,
  severityContent,
}: IllnessSeverityProps): JSX.Element {
  const { t } = useTranslation("illnessSeverity");
  // Use real data from backend if available, otherwise use defaults
  const [selectedSeverity, setSelectedSeverity] = useState(() => {
    if (severityContent) {
      // Try to extract severity from content (this would need more sophisticated parsing in real implementation)
      const content = severityContent.toLowerCase();
      if (content.includes("stable")) return "stable";
      if (content.includes("guarded")) return "guarded";
      if (content.includes("unstable")) return "unstable";
      if (content.includes("critical")) return "critical";
    }
    return "stable";
  });

  const [canEdit] = useState(
    assignedPhysician ? currentUser.name === assignedPhysician.name : true,
  );
  const [realtimeUpdate, setRealtimeUpdate] = useState(false);
  const [lastUpdated, setLastUpdated] = useState(t("justNow"));

  const severityLevels = severityLevelIds.map((id) => ({
    id,
    label: t(`levels.${id}.label`),
    description: t(`levels.${id}.description`),
    icon: severityIcons[id as keyof typeof severityIcons],
    ...severityStyling[id as keyof typeof severityStyling],
  }));

  const handleSeverityChange = (severityId: string): void => {
    if (canEdit) {
      setSelectedSeverity(severityId);
      setRealtimeUpdate(true);
      setLastUpdated(t("justNow"));

      // Simulate real-time update animation
      setTimeout(() => {
        setRealtimeUpdate(false);
      }, 2000);

      console.log(`Severity updated to: ${severityId}`);
    }
  };

  // Simulate receiving real-time updates from other users
  useEffect(() => {
    let interval: NodeJS.Timeout | undefined;

    if (!canEdit) {
      const simulateRealtimeUpdate = (): void => {
        setRealtimeUpdate(true);
        setTimeout(() => {
          setRealtimeUpdate(false);
        }, 1500);
      };

      // Simulate occasional updates for demo purposes
      interval = setInterval(() => {
        if (Math.random() > 0.95) {
          // 5% chance every second
          simulateRealtimeUpdate();
        }
      }, 1000);
    } else {
      // Clear any existing realtime updates when canEdit becomes true
      setRealtimeUpdate(false);
    }

    return (): void => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [canEdit]);

  return (
    <div className="space-y-3">
      {/* Real-time status and permissions header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <div
            className={`w-2 h-2 rounded-full ${realtimeUpdate ? "bg-green-500 animate-pulse" : "bg-gray-400"} transition-colors`}
          />
          <p className="text-sm text-gray-600">
            {t(canEdit ? "youCanEdit" : "liveAssessment")}
          </p>
          {realtimeUpdate && (
            <Badge
              className="text-xs text-green-600 border-green-200 bg-green-50 animate-pulse"
              variant="outline"
            >
              <Wifi className="w-3 h-3 mr-1" />
              {t("liveUpdate")}
            </Badge>
          )}
        </div>

        <Badge
          className={`text-xs ${canEdit ? "text-blue-600 border-blue-200 bg-blue-50" : "text-gray-600"}`}
          variant="outline"
        >
          {canEdit ? (
            <>
              <Edit className="w-3 h-3 mr-1" />
              {t("editor")}
            </>
          ) : (
            <>
              <Eye className="w-3 h-3 mr-1" />
              {t("viewer")}
            </>
          )}
        </Badge>
      </div>

      {/* Severity Selection Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        {severityLevels.map((level) => {
          const IconComponent = level.icon;
          const isSelected = selectedSeverity === level.id;
          const isClickable = canEdit;

          return (
            <button
              key={level.id}
              disabled={!isClickable}
              className={`medical-severity-option group relative p-3 rounded-lg border-2 text-left transition-all duration-150 ${
                isSelected
                  ? `${level.borderColor} ${level.bgColor} ${realtimeUpdate ? "realtime-update" : ""}`
                  : "border-gray-200 bg-white hover:border-gray-300"
              } ${!isClickable ? "cursor-default" : "cursor-pointer"}`}
              onClick={() => { handleSeverityChange(level.id); }}
            >
              <div className="flex items-center space-x-3">
                <div
                  className={`w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 transition-colors ${
                    isSelected
                      ? `${level.bgColor} border ${level.borderColor}`
                      : "bg-gray-100 group-hover:bg-gray-200"
                  }`}
                >
                  <IconComponent
                    className={`w-4 h-4 ${
                      isSelected
                        ? level.textColor
                        : "text-gray-500 group-hover:text-gray-700"
                    }`}
                  />
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <h5
                      className={`font-medium ${
                        isSelected
                          ? level.textColor
                          : "text-gray-900 group-hover:text-gray-800"
                      }`}
                    >
                      {level.label}
                    </h5>

                    {isSelected && (
                      <CheckCircle2
                        className={`w-4 h-4 ${level.textColor} flex-shrink-0`}
                      />
                    )}
                  </div>

                  <p
                    className={`text-sm mt-0.5 leading-relaxed ${
                      isSelected
                        ? level.textColor
                            .replace("600", "500")
                            .replace("700", "600")
                        : "text-gray-600 group-hover:text-gray-700"
                    }`}
                  >
                    {level.description}
                  </p>

                  {/* Assessment metadata - only for selected item */}
                  {isSelected && (
                    <div className="flex items-center space-x-3 text-xs text-gray-500 mt-2">
                      <span className="flex items-center space-x-1">
                        <User className="w-3 h-3" />
                        <span>{t("setBy", { user: assignedPhysician?.initials || 'Unknown' })}</span>
                      </span>
                      <span className="flex items-center space-x-1">
                        <Clock className="w-3 h-3" />
                        <span>{lastUpdated}</span>
                      </span>
                    </div>
                  )}
                </div>
              </div>
            </button>
          );
        })}
      </div>

      {/* Real-time collaboration status */}
      <div className="text-xs text-gray-500 text-center space-y-1">
        <p>
          {t(
            canEdit
              ? "changesSynced"
              : "onlyUserCanModify",
            {
              user: assignedPhysician?.initials || 'Unknown',
              role: assignedPhysician?.role || 'Unknown',
            },
          )}
        </p>
        {!canEdit && (
          <p className="text-gray-400">
            {t("updatesAutomatic")}
          </p>
        )}
      </div>
    </div>
  );
}
