import { AlertTriangle, CheckCircle, WifiOff } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

type SyncStatus = "synced" | "syncing" | "pending" | "offline" | "error";

export function useSyncStatus(): {
  isOnline: boolean;
  syncStatus: SyncStatus;
  setSyncStatus: (status: SyncStatus) => void;
  getSyncStatusDisplay: () => {
    icon: React.ReactNode;
    text: string;
    color: string;
  };
} {
  const { t } = useTranslation("header");
  const [isOnline, setIsOnline] = useState(true);
  const [syncStatus, setSyncStatus] = useState<SyncStatus>("synced");

  // Simulate network status changes
  useEffect((): (() => void) => {
    const handleOnline = (): void => {
      setIsOnline(true);
      setSyncStatus("syncing");
      setTimeout(() => {
        setSyncStatus("synced");
      }, 1500);
    };

    const handleOffline = (): void => {
      setIsOnline(false);
      setSyncStatus("offline");
    };

    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);

    return (): void => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
    };
  }, []);

  // Real-time collaboration sync status indicator
  const getSyncStatusDisplay = (): {
    icon: React.ReactNode;
    text: string;
    color: string;
  } => {
    switch (syncStatus) {
      case "syncing":
        return {
          icon: (
            <div className="w-2 h-2 bg-amber-500 rounded-full animate-pulse" />
          ),
          text: t("syncing"),
          color: "text-amber-600",
        };
      case "pending":
        return {
          icon: (
            <div className="w-2 h-2 bg-gray-400 rounded-full animate-pulse" />
          ),
          text: t("pending"),
          color: "text-gray-500",
        };
      case "offline":
        return {
          icon: <WifiOff className="w-3 h-3 text-red-500" />,
          text: t("offline"),
          color: "text-red-600",
        };
      case "error":
        return {
          icon: <AlertTriangle className="w-3 h-3 text-red-500" />,
          text: t("syncError"),
          color: "text-red-600",
        };
      default:
        return {
          icon: <CheckCircle className="w-3 h-3 text-green-500" />,
          text: t("allChangesSaved"),
          color: "text-green-600",
        };
    }
  };

  return {
    isOnline,
    syncStatus,
    setSyncStatus,
    getSyncStatusDisplay,
  };
}
