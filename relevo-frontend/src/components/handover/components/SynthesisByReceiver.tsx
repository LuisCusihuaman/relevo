import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  CheckCircle2,
  CheckSquare,
  Circle,
  Clock,
  Lock,
  ShieldCheck,
} from "lucide-react";
import { useEffect, useState, type JSX } from "react";
import { useTranslation } from "react-i18next";
import { useAllUsers } from "@/api/endpoints/users";
import { getInitials } from "@/lib/formatters";

interface SynthesisByReceiverProps {
  onComplete?: (completed: boolean) => void;
  onConfirm?: () => void;
  currentUser: {
    id?: string;
    name: string;
    initials: string;
    role: string;
  };
  receivingPhysician: {
    id?: string;
    name: string;
    initials: string;
    role: string;
  };
  assignedPhysician?: {
    id?: string;
    name: string;
    initials: string;
    role: string;
  };
  handoverState?: string;
  handoverComplete?: boolean;
  onReceiverChange?: (userId: string, userName: string) => void;
}

export function SynthesisByReceiver({
  onComplete,
  onConfirm,
  currentUser,
  receivingPhysician,
  assignedPhysician,
  handoverState,
  handoverComplete = false,
  onReceiverChange,
}: SynthesisByReceiverProps): JSX.Element {
  const { t } = useTranslation("synthesisByReceiver");
  const { data: allUsers = [], isLoading: isLoadingUsers } = useAllUsers();

  // Check if current user is the assigned physician (can select receiver)
  const isAssignedPhysician = assignedPhysician
    ? (currentUser.id ? currentUser.id === assignedPhysician.id : currentUser.name === assignedPhysician.name)
    : false;

  // Check if current user is the receiving physician
  const isReceiver = currentUser.name === receivingPhysician.name;

  // Selected receiver state
  const [selectedReceiverId, setSelectedReceiverId] = useState<string>(
    receivingPhysician.id || ""
  );

  // Confirmation checklist items
  const [confirmationItems, setConfirmationItems] = useState(() => [
    {
      id: "illness-severity",
      label: t("confirmationItems.illnessSeverity.label"),
      description: t("confirmationItems.illnessSeverity.description"),
      checked: false,
      required: true,
    },
    {
      id: "clinical-background",
      label: t("confirmationItems.clinicalBackground.label"),
      description: t("confirmationItems.clinicalBackground.description"),
      checked: false,
      required: true,
    },
    {
      id: "action-items",
      label: t("confirmationItems.actionItems.label"),
      description: t("confirmationItems.actionItems.description"),
      checked: false,
      required: true,
    },
    {
      id: "contingency-plans",
      label: t("confirmationItems.contingencyPlans.label"),
      description: t("confirmationItems.contingencyPlans.description"),
      checked: false,
      required: true,
    },
    {
      id: "questions-answered",
      label: t("confirmationItems.questionsAnswered.label"),
      description: t("confirmationItems.questionsAnswered.description"),
      checked: false,
      required: true,
    },
    {
      id: "accept-responsibility",
      label: t("confirmationItems.acceptResponsibility.label"),
      description: t("confirmationItems.acceptResponsibility.description"),
      checked: false,
      required: true,
      critical: true,
    },
  ]);

  // Get selected receiver display info (must be defined before use)
  const selectedReceiver = allUsers.find((u) => u.id === selectedReceiverId) || {
    id: receivingPhysician.id || "",
    fullName: receivingPhysician.name,
    firstName: "",
    lastName: "",
    email: "",
  };

  // Additional checks for confirmation permissions
  const handoverInProgress = handoverState === "InProgress";
  const handoverNotComplete = !handoverComplete;

  // Check if current user can confirm (must be receiver, handover in progress, and not already complete)
  // Update isReceiver check to use selected receiver
  const isCurrentUserReceiver = currentUser.name === selectedReceiver.fullName || 
    (currentUser.id && currentUser.id === selectedReceiver.id);
  const canConfirm = isCurrentUserReceiver && handoverInProgress && handoverNotComplete;

  // Calculate completion
  const completedItems = confirmationItems.filter(
    (item) => item.checked,
  ).length;
  const totalItems = confirmationItems.length;
  const isComplete = completedItems === totalItems;
  const completionProgress = (completedItems / totalItems) * 100;

  // Handle checkbox changes
  const handleItemChange = (itemId: string, checked: boolean): void => {
    if (!canConfirm) return;

    setConfirmationItems((previous) =>
      previous.map((item) => (item.id === itemId ? { ...item, checked } : item)),
    );
  };

  // Handle final confirmation
  const handleFinalConfirmation = (): void => {
    if (!canConfirm || !isComplete) return;

    // Mark handover as complete
    onConfirm?.();
    console.log("Handover officially confirmed by", receivingPhysician.name);
  };

  // Notify parent of completion status changes
  useEffect(() => {
    onComplete?.(isComplete);
  }, [isComplete, onComplete]);

  // Handle receiver selection change
  const handleReceiverChange = (userId: string): void => {
    setSelectedReceiverId(userId);
    const selectedUser = allUsers.find((u) => u.id === userId);
    if (selectedUser && onReceiverChange) {
      onReceiverChange(userId, selectedUser.fullName);
    }
  };

  return (
    <div className="space-y-6">
      {/* Receiving Physician Info */}
      <div className="flex items-center justify-between p-4 bg-purple-25 border border-purple-200 rounded-lg">
        <div className="flex items-center space-x-3">
          <Avatar className="w-10 h-10 border-2 border-purple-300">
            <AvatarFallback className="bg-purple-600 text-white">
              {getInitials(selectedReceiver.fullName)}
            </AvatarFallback>
          </Avatar>
          <div>
            {isAssignedPhysician && !handoverComplete ? (
              <Select
                value={selectedReceiverId}
                onValueChange={handleReceiverChange}
                disabled={isLoadingUsers}
              >
                <SelectTrigger className="w-[200px] h-auto py-1 border-purple-300 bg-white">
                  <SelectValue placeholder={t("selectReceiver", "Seleccionar receptor")}>
                    {selectedReceiver.fullName}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {allUsers.map((user) => (
                    <SelectItem key={user.id} value={user.id}>
                      {user.fullName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <>
                <h4 className="font-medium text-purple-900">
                  {selectedReceiver.fullName}
                </h4>
                <p className="text-sm text-purple-700">{receivingPhysician.role}</p>
              </>
            )}
          </div>
        </div>
        <div className="text-right">
          <p className="text-sm font-medium text-purple-900">
            {t("receivingPhysician")}
          </p>
          <p className="text-xs text-purple-700">{t("confirmationRequired")}</p>
        </div>
      </div>

      {/* Permission Notice */}
      {!canConfirm && (
        <div className="p-4 bg-amber-25 border border-amber-200 rounded-lg">
          <div className="flex items-center space-x-2 text-amber-800">
            <Lock className="w-4 h-4" />
            <span className="text-sm font-medium">
              {t("onlyReceiverConfirms", { name: selectedReceiver.fullName })}
            </span>
          </div>
          <p className="text-sm text-amber-700 mt-1">
            {t("receiverMustAccept")}
          </p>
        </div>
      )}

      {/* Progress Indicator */}
      {isCurrentUserReceiver && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="font-medium text-gray-900">
              {t("confirmationProgress")}
            </h4>
            <span className="text-sm text-gray-600">
              {t("progress", {
                completed: completedItems,
                total: totalItems,
              })}
            </span>
          </div>
          <Progress className="h-2" value={completionProgress} />
          {isComplete && (
            <p className="text-sm text-green-600 flex items-center space-x-1">
              <CheckCircle2 className="w-4 h-4" />
              <span>{t("allItemsConfirmed")}</span>
            </p>
          )}
        </div>
      )}

      {/* Confirmation Checklist */}
      <div className="space-y-4">
        <h4 className="font-medium text-gray-900 flex items-center space-x-2">
          <CheckSquare className="w-4 h-4 text-gray-600" />
          <span>{t("checklistTitle")}</span>
        </h4>

        <div className="space-y-4">
          {confirmationItems.map((item) => (
            <div
              key={item.id}
              className={`p-4 border rounded-lg transition-all ${
                item.checked
                  ? "border-green-200 bg-green-25"
                  : item.critical
                    ? "border-purple-200 bg-purple-25"
                    : "border-gray-200 bg-white hover:border-gray-300"
              } ${!canConfirm ? "opacity-60" : ""}`}
            >
              <div className="flex items-start space-x-3">
                <div className="flex-shrink-0 pt-1">
                  <Checkbox
                    checked={item.checked}
                    disabled={!canConfirm}
                    className={`${
                      item.critical ? "border-purple-400" : "border-gray-300"
                    } ${item.checked ? "bg-green-500 border-green-500" : ""}`}
                    onCheckedChange={(checked) =>
                      { handleItemChange(item.id, checked as boolean); }
                    }
                  />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between">
                    <label
                      className={`text-sm font-medium cursor-pointer ${
                        item.checked ? "text-green-800" : "text-gray-900"
                      } ${!canConfirm ? "cursor-not-allowed" : ""}`}
                    >
                      {item.label}
                      {item.critical && (
                        <Badge
                          className="ml-2 text-xs bg-purple-50 text-purple-700 border-purple-200"
                          variant="outline"
                        >
                          {t("critical")}
                        </Badge>
                      )}
                    </label>
                    {item.checked && (
                      <CheckCircle2 className="w-4 h-4 text-green-500 flex-shrink-0 ml-2" />
                    )}
                  </div>
                  <p
                    className={`text-xs mt-1 ${
                      item.checked ? "text-green-700" : "text-gray-600"
                    }`}
                  >
                    {item.description}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Final Confirmation Button */}
      {canConfirm && (
        <div className="p-4 bg-gray-25 border border-gray-200 rounded-lg">
          <div className="space-y-4">
            <div className="text-center">
              <h4 className="font-medium text-gray-900 mb-2">
                {t("finalConfirmation.title")}
              </h4>
              <p className="text-sm text-gray-600">
                {t("finalConfirmation.description")}
              </p>
            </div>

            <Button
              disabled={!isComplete}
              size="lg"
              className={`w-full ${
                isComplete
                  ? "bg-green-600 hover:bg-green-700 text-white"
                  : "bg-gray-300 text-gray-500 cursor-not-allowed"
              }`}
              onClick={handleFinalConfirmation}
            >
              {isComplete ? (
                <>
                  <ShieldCheck className="w-4 h-4 mr-2" />
                  {t("finalConfirmation.button.confirm")}
                </>
              ) : (
                <>
                  <Circle className="w-4 h-4 mr-2" />
                  {t("finalConfirmation.button.incomplete.prefix")}
                  {completedItems}/{totalItems}
                  {t("finalConfirmation.button.incomplete.suffix")}
                </>
              )}
            </Button>
          </div>
        </div>
      )}

      {/* Status Display for Non-Receiving Users */}
      {!isCurrentUserReceiver && !canConfirm && (
        <div className="p-4 bg-gray-25 border border-gray-200 rounded-lg text-center">
          <div className="space-y-2">
            <div className="flex items-center justify-center space-x-2">
              <Clock className="w-4 h-4 text-gray-500" />
              <span className="text-sm text-gray-600">
                {t("status.waitingFor", { name: selectedReceiver.fullName })}
              </span>
            </div>
            <div className="text-xs text-gray-500">
              {completedItems > 0
                ? t("status.itemsConfirmed", {
                    completed: completedItems,
                    total: totalItems,
                  })
                : t("status.pending")}
            </div>
          </div>
        </div>
      )}

      {/* Focus Mode - Read-Only Display */}
      {!isCurrentUserReceiver && (
        <div className="p-4 bg-gray-25 border border-gray-200 rounded-lg">
          <div className="text-center space-y-2">
            <h4 className="font-medium text-gray-900">{t("title")}</h4>
            <p className="text-sm text-gray-600">
              {t("description", { name: selectedReceiver.fullName })}
            </p>
            <div className="text-xs text-gray-500">
              {t("readOnly")}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
