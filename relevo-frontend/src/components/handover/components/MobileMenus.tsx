import type { User as UserType } from "@/common/types";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
} from "@/components/ui/sheet";
import {
    Activity,
    Clock,
    History,
    MessageSquare,
    User,
    Users,
} from "lucide-react";
import { useTranslation } from "react-i18next";
import { useIsMobile } from "@/hooks/use-mobile";
import { activeCollaborators } from "../../../common/constants";
import { CollaborationPanel } from "./CollaborationPanel";
import { HandoverHistory } from "./HandoverHistory";
import type { PatientHandoverData } from "../../../hooks/usePatientHandoverData";

interface MobileMenusProps {
  showMobileMenu: boolean;
  setShowMobileMenu: (show: boolean) => void;
  showHistory: boolean;
  setShowHistory: (show: boolean) => void;
  showComments: boolean;
  setShowComments: (show: boolean) => void;
  fullscreenEditing: boolean;
  getTimeUntilHandover: () => string;
  getSessionDuration: () => string;
  handleNavigateToSection: (section: string) => void;
  currentUser: UserType;
  handoverId?: string;
  participants?: any[];
  patientData: PatientHandoverData | null;
}

export function MobileMenus({
  showMobileMenu,
  setShowMobileMenu,
  showHistory,
  setShowHistory,
  showComments,
  setShowComments,
  fullscreenEditing,
  getTimeUntilHandover,
  getSessionDuration,
  handleNavigateToSection,
  currentUser: _currentUser,
  handoverId,
  participants: _participants,
  patientData,
}: MobileMenusProps): JSX.Element {
  const { t } = useTranslation("mobileMenus");
  const isMobile = useIsMobile();
  const activeUsers = activeCollaborators.filter(
    (user) => user.status === "active" || user.status === "viewing",
  );

  return (
    <>
      {/* Mobile Menu Sheet */}
      <Sheet open={showMobileMenu} onOpenChange={setShowMobileMenu}>
        <SheetContent
          className="w-80 bg-white border-l border-gray-200"
          side="right"
        >
          <SheetHeader className="pb-4 border-b border-gray-100">
            <SheetTitle className="text-left text-gray-900">
              {t("controls.title")}
            </SheetTitle>
            <SheetDescription className="text-left text-gray-600">
              {t("controls.description", { patientName: patientData?.name || "Patient" })}
            </SheetDescription>
          </SheetHeader>

          <div className="mt-4 space-y-4">
            {/* Patient Information */}
            <div className="bg-blue-50 border border-blue-200 p-3">
              <div className="flex items-center space-x-2 mb-3">
                <User className="w-4 h-4 text-blue-600" />
                <h4 className="text-sm font-medium text-blue-900">
                  {t("patientInfo.title")}
                </h4>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-blue-700">{t("patientInfo.age")}</span>
                  <span className="text-blue-900 font-medium">
                    {patientData?.age || "N/A"}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-blue-700">{t("patientInfo.mrn")}</span>
                  <span className="text-blue-900 font-mono text-xs">
                    {patientData?.mrn || "N/A"}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-blue-700">
                    {t("patientInfo.diagnosis")}
                  </span>
                  <span className="text-blue-900 font-medium text-right">
                    {patientData?.primaryDiagnosis || "N/A"}
                  </span>
                </div>
              </div>
            </div>

            {/* Session Status */}
            <div className="bg-gray-50 border border-gray-200 p-3">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                  <span className="text-sm font-medium text-gray-900">
                    {t("session.active")}
                  </span>
                </div>
                <span className="text-sm font-medium text-gray-900">
                  {getSessionDuration()}
                </span>
              </div>
              <div className="flex items-center space-x-2 text-xs text-gray-600">
                <Clock className="w-3 h-3" />
                <span>{t("session.untilHandover", { time: getTimeUntilHandover() })}</span>
              </div>
            </div>

            {/* Team Collaboration */}
            <div className="space-y-3">
              <div className="flex items-center space-x-2 px-1">
                <Users className="w-4 h-4 text-gray-700" />
                <h4 className="text-sm font-medium text-gray-900">
                  {t("collaboration.title")}
                </h4>
              </div>

              <div className="space-y-2">
                {/* Discussion - Now opens mobile Sheet */}
                <Button
                  className="w-full justify-start h-auto p-3"
                  variant={showComments ? "default" : "outline"}
                  onClick={() => {
                    setShowComments(true);
                    setShowMobileMenu(false);
                  }}
                >
                  <MessageSquare className="w-4 h-4 mr-3" />
                  <div className="text-left">
                    <div className="text-sm font-medium">
                      {t("collaboration.discussion")}
                    </div>
                    <div className="text-xs opacity-75">
                      {t("collaboration.discussionHint")}
                    </div>
                  </div>
                </Button>

                {/* History - Now opens mobile Sheet */}
                <Button
                  className="w-full justify-start h-auto p-3"
                  variant={showHistory ? "default" : "outline"}
                  onClick={() => {
                    setShowHistory(true);
                    setShowMobileMenu(false);
                  }}
                >
                  <History className="w-4 h-4 mr-3" />
                  <div className="text-left">
                    <div className="text-sm font-medium">
                      {t("collaboration.history")}
                    </div>
                    <div className="text-xs opacity-75">
                      {t("collaboration.historyHint")}
                    </div>
                  </div>
                </Button>
              </div>
            </div>


            {/* Active Team Members */}
            <div className="space-y-3">
              <div className="flex items-center justify-between px-1">
                <div className="flex items-center space-x-2">
                  <Activity className="w-4 h-4 text-gray-700" />
                  <h4 className="text-sm font-medium text-gray-900">
                    {t("activeTeam.title")}
                  </h4>
                </div>
                <Badge className="text-xs px-2 py-1" variant="outline">
                  {t("activeTeam.online", { count: activeUsers.length })}
                </Badge>
              </div>

              <div className="space-y-2">
                {activeUsers.slice(0, 3).map((user) => (
                  <div
                    key={user.id}
                    className="flex items-center space-x-3 p-2 border border-gray-200 bg-white"
                  >
                    <div className="relative">
                      <Avatar className="w-7 h-7">
                        <AvatarFallback
                          className={`${user.color} text-white text-xs`}
                        >
                          {user.initials}
                        </AvatarFallback>
                      </Avatar>
                      <div className="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 bg-green-500 rounded-full border border-white"></div>
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center space-x-2">
                        <p className="text-sm font-medium text-gray-900 truncate">
                          {user.name}
                        </p>
                        {user.presenceType === "assigned-current" && (
                          <Badge
                            className="text-xs px-1 py-0 bg-blue-50 text-blue-700 border-blue-200"
                            variant="outline"
                          >
                            {t("activeTeam.current")}
                          </Badge>
                        )}
                        {user.presenceType === "assigned-receiving" && (
                          <Badge
                            className="text-xs px-1 py-0 bg-purple-50 text-purple-700 border-purple-200"
                            variant="outline"
                          >
                            {t("activeTeam.receiving")}
                          </Badge>
                        )}
                      </div>
                      <p className="text-xs text-gray-600">{user.role}</p>
                    </div>
                  </div>
                ))}
                {activeUsers.length > 3 && (
                  <div className="text-xs text-gray-500 text-center py-2">
                    {t("activeTeam.moreActive", { count: activeUsers.length - 3 })}
                  </div>
                )}
              </div>
            </div>
          </div>
        </SheetContent>
      </Sheet>

      {/* Mobile History Sheet */}
      {showHistory && !isMobile && !fullscreenEditing && (
        <Sheet open={showHistory} onOpenChange={setShowHistory}>
          <SheetContent
            className="w-80 bg-white border-r border-gray-200 p-0"
            side="left"
          >
            <SheetHeader className="p-4 border-b border-gray-200">
              <SheetTitle className="text-left text-gray-900">
                {t("historySheet.title")}
              </SheetTitle>
              <SheetDescription className="text-left text-gray-600">
                {t("historySheet.description", { patientName: patientData?.name || "Patient" })}
              </SheetDescription>
            </SheetHeader>
            <div className="flex-1 overflow-auto">
              <HandoverHistory
                hideHeader
                patientData={patientData}
                handoverId={handoverId || ""}
                onClose={() => { setShowHistory(false); }}
              />
            </div>
          </SheetContent>
        </Sheet>
      )}

      {/* Mobile Collaboration Sheet */}
      {showComments && !isMobile && !fullscreenEditing && (
        <Sheet open={showComments} onOpenChange={setShowComments}>
          <SheetContent
            className="w-80 bg-white border-l border-gray-200 p-0"
            side="right"
          >
            <SheetHeader className="p-4 border-b border-gray-200">
              <SheetTitle className="text-left text-gray-900">
                {t("collaborationSheet.title")}
              </SheetTitle>
              <SheetDescription className="text-left text-gray-600">
                {t("collaborationSheet.description", {
                  patientName: patientData?.name || "Patient",
                })}
              </SheetDescription>
            </SheetHeader>
            <div className="flex-1 overflow-auto">
              <CollaborationPanel
                hideHeader
                handoverId={handoverId || ""}
                onClose={() => { setShowComments(false); }}
                onNavigateToSection={handleNavigateToSection}
              />
            </div>
          </SheetContent>
        </Sheet>
      )}
    </>
  );
}
