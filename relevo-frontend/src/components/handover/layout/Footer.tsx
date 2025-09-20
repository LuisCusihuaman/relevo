import { patientData } from "@/common/constants";
import { Button } from "@/components/ui/button";
import { useTranslation } from "react-i18next";

interface FooterProps {
  focusMode: boolean;
  fullscreenEditing: boolean;
  handoverComplete: boolean;
  getTimeUntilHandover: () => string;
  getSessionDuration: () => string;
}

export function Footer({
  focusMode,
  fullscreenEditing,
  handoverComplete,
  getTimeUntilHandover,
  getSessionDuration,
}: FooterProps) {
  const { t } = useTranslation("handover");
  if (focusMode || fullscreenEditing) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 px-4 sm:px-6 lg:px-8 py-4 z-30">
      <div className="w-full max-w-none mx-auto flex flex-col sm:flex-row sm:items-center justify-between space-y-3 sm:space-y-0">
        <div className="flex items-center space-x-4 text-sm text-gray-600">
          <span>{t("handoverAt", { time: patientData.handoverTime })}</span>
          <span>•</span>
          <span>{t("remaining", { time: getTimeUntilHandover() })}</span>
          <span>•</span>
          <span>{t("session", { duration: getSessionDuration() })}</span>
        </div>
        <Button
          disabled={handoverComplete}
          size="sm"
          className={`w-full sm:w-auto ${
            handoverComplete
              ? "bg-green-600 hover:bg-green-700 text-white"
              : "bg-gray-900 hover:bg-gray-800 text-white"
          }`}
        >
          {handoverComplete
            ? t("handoverComplete")
            : t("completeHandover")}
        </Button>
      </div>
    </div>
  );
}
