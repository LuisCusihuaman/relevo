import { Button } from "@/components/ui/button";
import { useTranslation } from "react-i18next";
import type { PatientHandoverData } from "@/hooks/usePatientHandoverData";
import type { Handover } from "@/api/types";
import { type CurrentUserData } from "@/hooks/useCurrentUser";

interface FooterProps {
  focusMode: boolean;
  fullscreenEditing: boolean;
  handoverComplete: boolean;
  getTimeUntilHandover: () => string;
  getSessionDuration: () => string;
  patientData: PatientHandoverData | null;
  handover?: Handover | null;
  currentUser?: CurrentUserData | null;
  onReady?: () => void;
  onStart?: () => void;
  onAccept?: () => void;
  onComplete?: () => void;
  onCancel?: () => void;
  onReject?: () => void;
}

export function Footer({
  focusMode,
  fullscreenEditing,
  getTimeUntilHandover,
  getSessionDuration,
  patientData,
  handover,
  currentUser,
  onAccept,
  onCancel,
  onComplete,
  onReady,
  onReject,
  onStart,
}: FooterProps): JSX.Element | null {
  const { t } = useTranslation("handover");

  if (focusMode || fullscreenEditing) return null;

  const isSender = handover?.createdBy === currentUser?.name; // TODO: Compare IDs when available on currentUser

  const renderButtons = () => {
    switch (handover?.stateName) {
      case "Draft":
        return isSender && <Button onClick={onReady}>{t("footer.readyForHandover")}</Button>;
      case "Ready":
        return <Button onClick={onStart}>{t("footer.startHandover")}</Button>;
      case "InProgress":
        return (
          <>
            <Button variant="outline" onClick={onReject}>
              {t("footer.reject")}
            </Button>
            <Button onClick={onAccept}>{t("footer.acceptHandover")}</Button>
          </>
        );
      case "Accepted":
        return <Button onClick={onComplete}>{t("footer.completeHandover")}</Button>;
      case "Completed":
        return (
          <Button disabled className="bg-green-600 hover:bg-green-700 text-white">
            {t("footer.handoverComplete")}
          </Button>
        );
      default:
        return null;
    }
  };

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 px-4 sm:px-6 lg:px-8 py-4 z-30">
      <div className="w-full max-w-none mx-auto flex flex-col sm:flex-row sm:items-center justify-between space-y-3 sm:space-y-0">
        <div className="flex items-center space-x-4 text-sm text-gray-600">
          <span>{t("handoverAt", { time: patientData?.handoverTime || "N/A" })}</span>
          <span>•</span>
          <span>{t("remaining", { time: getTimeUntilHandover() })}</span>
          <span>•</span>
          <span>{t("session", { duration: getSessionDuration() })}</span>
        </div>
        <div className="flex items-center space-x-2">{renderButtons()}</div>
      </div>
    </div>
  );
}
