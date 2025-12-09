import type { JSX } from "react";
import {
  activeCollaborators,
  currentlyPresent,
} from "@/common/constants";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  Activity,
  ArrowRight,
  Calendar,
  ChevronLeft,
  FileText,
  History,
  MapPin,
  MessageSquare,
  MoreHorizontal,
  UserPlus,
} from "lucide-react";
import { useTranslation } from "react-i18next";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { useSyncStatus } from "@/components/handover/hooks/useSyncStatus";
import { useHandoverSession } from "@/components/handover/hooks/useHandoverSession";
import { useCurrentHandover, HandoverStatusControls } from "@/components/handover";

interface HeaderProps {
  onBack?: () => void;
}

export function Header({
  onBack,
}: HeaderProps): JSX.Element {
  const { t } = useTranslation(["header", "handover", "patientHeader"]);
  
  // UI Store
  const {
    showCollaborators,
    setShowCollaborators,
    showComments,
    setShowComments,
    showHistory,
    setShowHistory,
    setShowMobileMenu,
  } = useHandoverUIStore();

  // Hooks
  const { getSyncStatusDisplay } = useSyncStatus();
  const { getTimeUntilHandover, getSessionDuration } = useHandoverSession();
  
  // Data
  const { 
    patientData, 
    handoverData, 
    currentUser, 
    assignedPhysician, 
    receivingPhysician 
  } = useCurrentHandover();

  // Determine roles for controls
  const isSender = !!assignedPhysician.id && currentUser.id === assignedPhysician.id;
  // Receiver logic: Use ID if available, otherwise name match as fallback (legacy)
  const isReceiver = receivingPhysician.id 
    ? currentUser.id === receivingPhysician.id
    : currentUser.name === receivingPhysician.name;

  // Calculate age from DOB
  const calculateAgeFromDob = (dobString: string): number => {
    if (!dobString) return 0;

    try {
      const birthDate = new Date(dobString);
      const today = new Date();
      let age = today.getFullYear() - birthDate.getFullYear();
      const monthDiff = today.getMonth() - birthDate.getMonth();

      // Adjust if birthday hasn't occurred this year
      if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        age--;
      }

      return age;
    } catch {
      return 0;
    }
  };

  // Generate initials from name
  const getInitials = (name: string): string => {
    if (!name) return "?";
    return name
      .split(" ")
      .filter(n => n.length > 0)
      .map(n => n[0])
      .slice(0, 2)
      .join("")
      .toUpperCase();
  };
  
  const activeUsers = activeCollaborators.filter(
    (user) => user.status === "active" || user.status === "viewing",
  );

  return (
    <div className="bg-white border-b border-gray-200 sticky top-0 z-40">
      {/* Main Header */}
      <div className="px-4 sm:px-6 py-3 border-b border-gray-100">
        <div className="flex items-center justify-between w-full max-w-none mx-auto">
          {/* Left Section - Logo + Patient Info */}
          <div className="flex items-center space-x-4 sm:space-x-6 min-w-0 flex-1">
            {/* Back Button - Only show if onBack prop provided */}
            {onBack && (
              <Button
                className="flex-shrink-0"
                size="sm"
                variant="ghost"
                onClick={onBack}
              >
                <ChevronLeft className="w-4 h-4 mr-1" />
                <span className="hidden sm:inline">{t("back")}</span>
              </Button>
            )}

            {/* Patient Name + Essential Info */}
            <div className="flex items-center space-x-3 min-w-0 flex-1">
              <h2 className="font-medium text-gray-900 truncate">
                {patientData?.name || "Unknown Patient"}
              </h2>
              <Badge
                className="text-gray-700 border-gray-200 bg-gray-50 flex-shrink-0"
                variant="outline"
              >
                {patientData?.room || "Unknown"}
              </Badge>

              {/* Session duration - realistic medical tracking */}
              <div className="hidden xl:flex items-center space-x-2">
                <div className="w-2 h-2 rounded-full bg-green-500"></div>
                <span className="text-xs text-gray-600">
                  {t("session", { duration: getSessionDuration() })}
                </span>
              </div>
            </div>
          </div>

          {/* Center - Google Docs Style Collaborators with Tooltips */}
          <div className="flex items-center space-x-1">
            {/* Currently Present - Show first 2 active with tooltips */}
            {activeUsers.slice(0, 2).map((user) => (
              <Tooltip key={user.id}>
                <TooltipTrigger asChild>
                  <div className="relative cursor-pointer">
                    <Avatar className="w-8 h-8 border-2 border-white hover:border-gray-200 transition-colors">
                      <AvatarFallback
                        className={`${user.color} text-white text-xs`}
                      >
                        {user.initials}
                      </AvatarFallback>
                    </Avatar>
                    {/* Live indicator */}
                    <div className="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 bg-green-500 rounded-full border border-white"></div>
                  </div>
                </TooltipTrigger>
                <TooltipContent
                  className="bg-gray-900 text-white text-xs px-2 py-1 border-none shadow-lg"
                  side="bottom"
                >
                  <div className="text-center">
                    <div className="font-medium">{user.name}</div>
                    <div className="text-gray-300">{user.role}</div>
                  </div>
                </TooltipContent>
              </Tooltip>
            ))}

            {/* Show More Button - Google Docs Style */}
            {activeUsers.length > 2 && (
              <Popover
                open={showCollaborators}
                onOpenChange={setShowCollaborators}
              >
                <PopoverTrigger asChild>
                  <Button
                    className="h-8 w-auto px-2 text-xs text-gray-600 hover:bg-gray-100 rounded-full"
                    size="sm"
                    variant="ghost"
                  >
                    +{activeUsers.length - 2}
                  </Button>
                </PopoverTrigger>
                <PopoverContent
                  align="center"
                  className="w-72 p-0 bg-white border-gray-200 shadow-lg"
                >
                  <div className="p-3 border-b border-gray-100">
                    <h3 className="font-medium text-gray-900 text-sm">
                      {t("peopleWithAccess")}
                    </h3>
                    <p className="text-xs text-gray-600">
                      {t("peopleCanViewAndEdit", {
                        count: activeUsers.length,
                      })}
                    </p>
                  </div>
                  <div className="max-h-64 overflow-y-auto">
                    {currentlyPresent.map((person) => (
                      <div
                        key={person.id}
                        className="p-3 hover:bg-gray-50 transition-colors border-b border-gray-50"
                      >
                        <div className="flex items-center space-x-3">
                          <div className="relative">
                            <Avatar className="w-8 h-8">
                              <AvatarFallback
                                className={`${person.color} text-white text-sm`}
                              >
                                {person.initials}
                              </AvatarFallback>
                            </Avatar>
                            <div
                              className={`absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-white ${
                                person.status === "active"
                                  ? "bg-green-500"
                                  : person.status === "viewing"
                                  ? "bg-amber-500"
                                  : "bg-gray-400"
                              }`}
                            ></div>
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center space-x-2">
                              <p className="text-sm font-medium text-gray-900">
                                {person.name}
                              </p>
                              {person.presenceType === "assigned-current" && (
                                <Badge
                                  className="text-xs px-1 py-0 bg-blue-50 text-blue-700 border-blue-200"
                                  variant="outline"
                                >
                                  {t("current")}
                                </Badge>
                              )}
                              {person.presenceType === "assigned-receiving" && (
                                <Badge
                                  className="text-xs px-1 py-0 bg-purple-50 text-purple-700 border-purple-200"
                                  variant="outline"
                                >
                                  {t("receiving")}
                                </Badge>
                              )}
                            </div>
                            <p className="text-xs text-gray-500">
                              {person.role}
                            </p>
                            <div className="flex items-center space-x-1 mt-1">
                              <div
                                className={`w-1.5 h-1.5 rounded-full ${
                                  person.status === "active"
                                    ? "bg-green-500"
                                    : person.status === "viewing"
                                    ? "bg-amber-500"
                                    : "bg-gray-400"
                                }`}
                              ></div>
                              <span className="text-xs text-gray-500">
                                {person.status === "active"
                                  ? t("activeNow")
                                  : person.status === "viewing"
                                  ? "Viewing now"
                                  : `Last seen ${person.lastSeen}`}
                              </span>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="p-3 border-t border-gray-100">
                    <Button
                      className="w-full text-xs hover:bg-gray-50 justify-center"
                      size="sm"
                      variant="ghost"
                    >
                      <UserPlus className="w-3 h-3 mr-2" />
                      Share with others
                    </Button>
                  </div>
                </PopoverContent>
              </Popover>
            )}
          </div>

          {/* Right Section - Controls */}
          <div className="flex items-center space-x-2 sm:space-x-3 flex-shrink-0">
            {/* Status Controls */}
            {handoverData && (
              <HandoverStatusControls
                handover={handoverData}
                currentUserId={currentUser.id}
                isSender={isSender}
                isReceiver={isReceiver}
              />
            )}

            {/* Sync status indicator */}
            <div className="hidden md:flex items-center space-x-2 text-xs text-gray-600">
              {getSyncStatusDisplay().icon}
              <span className={getSyncStatusDisplay().color}>
                {getSyncStatusDisplay().text}
              </span>
            </div>


            {/* Mobile Menu */}
            <Button
              className="md:hidden h-8 w-8 p-0 hover:bg-gray-100"
              size="sm"
              variant="ghost"
              onClick={() => { setShowMobileMenu(true); }}
            >
              <MoreHorizontal className="w-4 h-4" />
            </Button>

            {/* Desktop Controls */}
            <div className="hidden md:flex items-center space-x-2">
              {/* Discussion */}
              <Button
                size="sm"
                variant="ghost"
                className={`text-xs p-2 hover:bg-gray-100 ${
                  showComments ? "bg-gray-100" : ""
                }`}
                onClick={() => { setShowComments(!showComments); }}
              >
                <MessageSquare className="w-4 h-4" />
              </Button>
              {/* History */}
              <Button
                size="sm"
                variant="ghost"
                className={`text-xs p-2 hover:bg-gray-100 ${
                  showHistory ? "bg-gray-100" : ""
                }`}
                onClick={() => { setShowHistory(!showHistory); }}
              >
                <History className="w-4 h-4" />
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Patient Info Bar - Continuous with header (no gap) */}
      <div className="px-4 sm:px-6 lg:px-8 py-3">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between space-y-2 sm:space-y-0">
          <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
            <div className="flex items-center space-x-1">
              <Calendar className="w-3 h-3" />
              <span>{t("age", { age: calculateAgeFromDob(patientData?.dob || ""), ns: "patientHeader" })}</span>
            </div>
            <div className="flex items-center space-x-1">
              <FileText className="w-3 h-3" />
              <span className="font-mono text-xs">{t("mrn", { mrn: patientData?.mrn || "Unknown", ns: "patientHeader" })}</span>
            </div>
            <div className="flex items-center space-x-1">
              <MapPin className="w-3 h-3" />
              <span>{patientData?.unit || "Unknown"}</span>
            </div>
            <div className="flex items-center space-x-1">
              <Activity className="w-3 h-3" />
              <span>
                {patientData?.primaryDiagnosis
                  ? (patientData.primaryDiagnosis.includes('.')
                    ? t(patientData.primaryDiagnosis, { ns: 'patientHeader' })
                    : patientData.primaryDiagnosis)
                  : "Unknown"}
              </span>
            </div>
          </div>
          <div className="flex items-center space-x-3 text-sm">
            <span className="text-gray-500">{t("handover")}:</span>
            <div className="flex items-center space-x-2">
              <Tooltip>
                <TooltipTrigger asChild>
                  <Avatar className="w-5 h-5 cursor-pointer hover:ring-2 hover:ring-blue-200">
                    <AvatarFallback
                      className={`${patientData?.assignedPhysician?.color || "bg-gray-400"} text-white text-xs`}
                    >
                      {getInitials(patientData?.assignedPhysician?.name || "")}
                    </AvatarFallback>
                  </Avatar>
                </TooltipTrigger>
                <TooltipContent
                  className="bg-gray-900 text-white text-xs px-2 py-1 border-none shadow-lg"
                  side="bottom"
                >
                  <div className="text-center">
                    <div className="font-medium">
                      {patientData?.assignedPhysician?.name || "Physician data not available"}
                    </div>
                    <div className="text-gray-300">
                      {patientData?.assignedPhysician?.role || "Role not available"}
                    </div>
                  </div>
                </TooltipContent>
              </Tooltip>
              <ArrowRight className="w-3 h-3 text-gray-400" />
              <Tooltip>
                <TooltipTrigger asChild>
                  <Avatar className="w-5 h-5 cursor-pointer hover:ring-2 hover:ring-purple-200">
                    <AvatarFallback
                      className={`${patientData?.receivingPhysician?.color || "bg-gray-400"} text-white text-xs`}
                    >
                      {getInitials(patientData?.receivingPhysician?.name || "")}
                    </AvatarFallback>
                  </Avatar>
                </TooltipTrigger>
                <TooltipContent
                  className="bg-gray-900 text-white text-xs px-2 py-1 border-none shadow-lg"
                  side="bottom"
                >
                  <div className="text-center">
                    <div className="font-medium">
                      {patientData?.receivingPhysician?.name || "Physician data not available"}
                    </div>
                    <div className="text-gray-300">
                      {patientData?.receivingPhysician?.role || "Role not available"}
                    </div>
                  </div>
                </TooltipContent>
              </Tooltip>
              <span className="text-gray-500">{getTimeUntilHandover()}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
