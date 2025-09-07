import type { FC } from "react";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { AlertTriangle, Bell, Settings } from "lucide-react";
import { useTranslation } from "react-i18next";

export type NotificationItem = {
  id: string;
  kind: "warning" | "info";
  titleKey: string;
  titleParams?: Record<string, string>;
  bodyKey?: string;
  bodyParams?: Record<string, string>;
  time: string;
  withAvatar?: boolean;
  avatarFallback?: string;
};

type NotificationsPopoverProps = {
  isMobileMenuOpen: boolean;
  items?: Array<NotificationItem>;
};

export const NotificationsPopover: FC<NotificationsPopoverProps> = ({
  isMobileMenuOpen,
  items,
}) => {
  const { t } = useTranslation("home");

  const data: Array<NotificationItem> =
    items ?? [
      {
        id: "1",
        kind: "warning",
        titleKey: "notifications.failedHandover",
        titleParams: { patient: "Ana Pérez", env: String(t("notifications.env.test")) },
        time: "hace 2d",
      },
      {
        id: "2",
        kind: "warning",
        titleKey: "notifications.failedHandover",
        titleParams: { patient: "Carlos Gómez", env: String(t("notifications.env.test")) },
        time: "hace 2d",
      },
      {
        id: "3",
        kind: "info",
        titleKey: "notifications.patientAttentionTitle",
        titleParams: { patient: "Juan Rodríguez", date: "1 de Septiembre de 2025" },
        bodyKey: "notifications.patientAttentionBody",
        time: "7 Ago",
        withAvatar: true,
        avatarFallback: "LC",
      },
    ];

  return (
    <Popover>
      <PopoverTrigger asChild>
        <div className={`relative ${isMobileMenuOpen ? "hidden" : ""}`}>
          <Button className="h-8 w-8 p-0 text-gray-600 hover:text-gray-900" size="sm" variant="ghost">
            <Bell className="h-4 w-4" />
          </Button>
          <div className="absolute -top-1 -right-1 h-2 w-2 bg-blue-500 rounded-full"></div>
        </div>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-96 p-0 z-50">
        <div className="border-b border-gray-100">
          <div className="flex items-center justify-between px-4 py-3">
            <div className="flex items-center gap-6">
              <button className="text-sm font-medium text-gray-900 border-b-2 border-black pb-1">
                {t("notifications.inbox")}
              </button>
              <button className="text-sm text-gray-600 hover:text-gray-900 pb-1">
                {t("notifications.archived")}
              </button>
              <button className="text-sm text-gray-600 hover:text-gray-900 pb-1">
                {t("notifications.comments")}
              </button>
            </div>
            <Button className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600" size="sm" variant="ghost">
              <Settings className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <div className="max-h-96 overflow-y-auto">
          {data.map((n) => (
            <div key={n.id} className="px-4 py-3 border-b border-gray-100">
              <div className="flex items-center gap-3">
                {n.withAvatar ? (
                  <Avatar className="h-8 w-8 flex-shrink-0">
                    <AvatarImage src="/placeholder.svg?height=32&width=32" />
                    <AvatarFallback>{n.avatarFallback ?? ""}</AvatarFallback>
                  </Avatar>
                ) : (
                  <div className="h-8 w-8 rounded-full bg-orange-100 flex items-center justify-center flex-shrink-0">
                    <AlertTriangle className="h-4 w-4 text-orange-600" />
                  </div>
                )}
                <div className="flex-1 min-w-0">
                  <p className="text-sm text-gray-900 font-medium">
                    {t(n.titleKey, n.titleParams)}
                  </p>
                  {n.bodyKey && (
                    <p className="text-sm text-gray-600 mt-1">{t(n.bodyKey, n.bodyParams)}</p>
                  )}
                  <p className="text-xs text-gray-500 mt-1">{n.time}</p>
                </div>
                <Button className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600 flex-shrink-0" size="sm" variant="ghost">
                  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} />
                  </svg>
                </Button>
              </div>
            </div>
          ))}

          <div className="border-t border-gray-100 px-4 py-2">
            <Button className="w-full text-sm text-gray-600 hover:text-gray-900 justify-center" variant="ghost">
              {t("notifications.archiveAll")}
            </Button>
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
};

export default NotificationsPopover;


