import type { JSX } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Trash2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { ContingencyPlan } from "@/types/domain";

interface ContingencyPlanListProps {
  plans: Array<ContingencyPlan>;
  canDelete: boolean;
  onDelete: (id: string) => void;
}

// Priority colors
const getPriorityColor = (priority: string): string => {
  switch (priority) {
    case "high":
      return "text-red-600 border-red-200";
    case "medium":
      return "text-amber-600 border-amber-200";
    case "low":
      return "text-emerald-600 border-emerald-200";
    default:
      return "text-gray-600 border-gray-200";
  }
};

// Status badge styling
const getStatusBadge = (status: string): string => {
  switch (status) {
    case "active":
      return "bg-emerald-50 text-emerald-700 border-emerald-200";
    case "planned":
      return "bg-blue-50 text-blue-700 border-blue-200";
    default:
      return "bg-gray-50 text-gray-700 border-gray-200";
  }
};

export function ContingencyPlanList({ plans, canDelete, onDelete }: ContingencyPlanListProps): JSX.Element {
  const { t } = useTranslation("situationAwareness");

  return (
    <div className="space-y-3">
      {plans.map((plan) => (
        <div
          key={plan.id}
          className="p-4 rounded-lg border border-gray-200 bg-white hover:border-gray-300 transition-all group"
        >
          <div className="space-y-3">
            {/* Plan Header */}
            <div className="flex items-start justify-between">
              <div className="flex items-start space-x-3 flex-1 min-w-0">
                <div
                  className={`text-xs px-2 py-1 rounded border font-medium ${getPriorityColor(plan.priority)}`}
                >
                  {plan.priority.toUpperCase()}
                </div>
                <div className="flex-1 min-w-0">
                  <Badge
                    className={`text-xs border ${getStatusBadge(plan.status)} mb-2`}
                  >
                    {plan.status === "active"
                      ? t("status.active")
                      : t("status.planned")}
                  </Badge>
                </div>
              </div>

              {/* Delete button - Only for assigned physician */}
              {canDelete && (
                <Button
                  className="opacity-0 group-hover:opacity-100 transition-opacity text-red-600 hover:text-red-700 hover:bg-red-50 h-6 w-6 p-0"
                  size="sm"
                  variant="ghost"
                  onClick={(event) => {
                    event.stopPropagation();
                    onDelete(plan.id);
                  }}
                >
                  <Trash2 className="w-3 h-3" />
                </Button>
              )}
            </div>

            {/* Clean IF/THEN Content - Final Version */}
            <div className="space-y-2">
              <div className="flex items-start space-x-2">
                <span className="text-sm font-medium text-gray-700 flex-shrink-0">
                  {t("contingencyPlanning.ifPrefix")}
                </span>
                <span className="text-sm text-gray-900">
                  {plan.condition}
                </span>
              </div>
              <div className="flex items-start space-x-2">
                <span className="text-sm font-medium text-gray-700 flex-shrink-0">
                  {t("contingencyPlanning.thenPrefix")}
                </span>
                <span className="text-sm text-gray-900">
                  {plan.action}
                </span>
              </div>
            </div>

            {/* Plan Footer - Submitted info */}
            <div className="flex items-center justify-between text-xs text-gray-500 pt-2 border-t border-gray-100">
              <span>
                {t("submittedBy", {
                  name: plan.createdBy,
                  time: new Date(plan.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
                  date: new Date(plan.createdAt).toLocaleDateString(),
                })}
              </span>
              <span>{new Date(plan.createdAt).toLocaleDateString()}</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

