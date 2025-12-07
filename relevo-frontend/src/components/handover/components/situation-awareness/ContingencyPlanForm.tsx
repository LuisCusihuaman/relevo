import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Send, X } from "lucide-react";
import { type ChangeEvent, type KeyboardEvent, type JSX, useState } from "react";
import { useTranslation } from "react-i18next";

export interface NewPlanData {
  condition: string;
  action: string;
  priority: "low" | "medium" | "high";
}

interface ContingencyPlanFormProps {
  onSubmit: (data: NewPlanData) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
}

export function ContingencyPlanForm({ onSubmit, onCancel, isSubmitting }: ContingencyPlanFormProps): JSX.Element {
  const { t } = useTranslation("situationAwareness");
  const [newPlan, setNewPlan] = useState<NewPlanData>({
    condition: "",
    action: "",
    priority: "medium",
  });

  const handleSubmit = async (): Promise<void> => {
    if (!newPlan.condition || !newPlan.action) return;
    await onSubmit(newPlan);
    // Reset form is handled by parent or here if successful, but parent usually closes or resets
  };

  const handleKeyDown = (event: KeyboardEvent): void => {
    if (event.key === "Enter" && (event.metaKey || event.ctrlKey)) {
      event.preventDefault();
      void handleSubmit();
    }
  };

  return (
    <div className="p-4 border border-gray-200 rounded-lg bg-gray-25">
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h5 className="font-medium text-gray-900">
            {t("newPlan.title")}
          </h5>
          <Button
            className="h-6 w-6 p-0"
            disabled={isSubmitting}
            size="sm"
            variant="ghost"
            onClick={onCancel}
          >
            <X className="w-4 h-4" />
          </Button>
        </div>

        <div className="space-y-3">
          <div>
            <label
              className="text-sm font-medium text-gray-700 mb-1 block"
              htmlFor="plan-condition"
            >
              {t("contingencyPlanning.form.conditionLabel")}
            </label>
            <Textarea
              className="w-full p-2 border border-gray-300 rounded-md text-sm bg-white"
              id="plan-condition"
              rows={2}
              value={newPlan.condition}
              placeholder={t(
                "contingencyPlanning.form.conditionPlaceholder",
              )}
              onKeyDown={handleKeyDown}
              onChange={(event: ChangeEvent<HTMLTextAreaElement>) =>
                { setNewPlan({ ...newPlan, condition: event.target.value }); }
              }
            />
          </div>

          <div>
            <label
              className="text-sm font-medium text-gray-700 mb-1 block"
              htmlFor="plan-action"
            >
              {t("contingencyPlanning.form.actionLabel")}
            </label>
            <Textarea
              className="min-h-[60px] border-gray-300 focus:border-blue-400 focus:ring-blue-100 bg-white"
              disabled={isSubmitting}
              id="plan-action"
              value={newPlan.action}
              placeholder={t(
                "contingencyPlanning.form.actionPlaceholder",
              )}
              onKeyDown={handleKeyDown}
              onChange={(event: ChangeEvent<HTMLTextAreaElement>) =>
                { setNewPlan({ ...newPlan, action: event.target.value }); }
              }
            />
          </div>

          <div>
            <label
              className="text-sm font-medium text-gray-700 mb-1 block"
              htmlFor="plan-priority"
            >
              {t("contingencyPlanning.form.priorityLabel")}
            </label>
            <select
              className="w-full p-2 text-sm border border-gray-300 rounded-lg bg-white focus:border-blue-400 focus:ring-blue-100"
              disabled={isSubmitting}
              id="plan-priority"
              value={newPlan.priority}
              onChange={(event: ChangeEvent<HTMLSelectElement>) =>
                { setNewPlan({ ...newPlan, priority: event.target.value as NewPlanData["priority"] }); }
              }
            >
              <option value="low">{t("priorities.low")}</option>
              <option value="medium">{t("priorities.medium")}</option>
              <option value="high">{t("priorities.high")}</option>
            </select>
          </div>
        </div>

        <div className="flex justify-end space-x-2">
          <Button
            className="text-xs border-gray-300 hover:bg-gray-50"
            disabled={isSubmitting}
            size="sm"
            variant="outline"
            onClick={onCancel}
          >
            {t("cancel")}
          </Button>
          <Button
            className="text-xs bg-blue-600 hover:bg-blue-700 text-white"
            size="sm"
            disabled={
              !newPlan.condition || !newPlan.action || isSubmitting
            }
            onClick={() => { void handleSubmit(); }}
          >
            {isSubmitting ? (
              <>
                <div className="w-3 h-3 border border-white border-t-transparent rounded-full animate-spin mr-1"></div>
                {t("newPlan.submitting")}
              </>
            ) : (
              <>
                <Send className="w-3 h-3 mr-1" />
                {t("newPlan.submit")}
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}

