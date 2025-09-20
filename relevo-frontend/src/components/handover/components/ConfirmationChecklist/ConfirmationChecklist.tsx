import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Progress } from "@/components/ui/progress";
import { AlertCircle, CheckCircle, Clock } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

interface ConfirmationChecklistProps {
  onComplete: (complete: boolean) => void;
}

export function ConfirmationChecklist({
  onComplete,
}: ConfirmationChecklistProps): JSX.Element {
  const { t } = useTranslation("confirmationChecklist");
  const [checkedItems, setCheckedItems] = useState<{ [key: string]: boolean }>(
    {},
  );

  const checklistItems = [
    {
      id: "clinical-status",
      category: t("categories.clinicalStatus"),
      item: t("items.reviewClinicalData"),
      required: true,
    },
    {
      id: "medications",
      category: t("categories.medications"),
      item: t("items.understandMedications"),
      required: true,
    },
    {
      id: "allergies",
      category: t("categories.safety"),
      item: t("items.noteAllergies"),
      required: true,
    },
    {
      id: "priorities",
      category: t("categories.carePlan"),
      item: t("items.understandPriorities"),
      required: true,
    },
    {
      id: "contingency",
      category: t("categories.contingency"),
      item: t("items.awareOfContingency"),
      required: true,
    },
    {
      id: "communication",
      category: t("categories.communication"),
      item: t("items.knowWhoToContact"),
      required: true,
    },
    {
      id: "documentation",
      category: t("categories.documentation"),
      item: t("items.documentationComplete"),
      required: false,
    },
    {
      id: "family",
      category: t("categories.family"),
      item: t("items.awareOfFamilyNeeds"),
      required: false,
    },
  ];

  const handleItemCheck = (itemId: string, checked: boolean): void => {
    setCheckedItems((previous) => ({ ...previous, [itemId]: checked }));
  };

  const requiredItems = checklistItems.filter((item) => item.required);
  const requiredChecked = requiredItems.filter(
    (item) => checkedItems[item.id],
  ).length;
  const totalChecked = checklistItems.filter(
    (item) => checkedItems[item.id],
  ).length;
  const allRequiredComplete = requiredChecked === requiredItems.length;
  const progressPercentage = (totalChecked / checklistItems.length) * 100;

  useEffect(() => {
    onComplete(allRequiredComplete);
  }, [allRequiredComplete, onComplete]);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <CheckCircle className="w-5 h-5 text-blue-600" />
            <span>{t("title")}</span>
          </div>
          <Badge variant={allRequiredComplete ? "default" : "secondary"}>
            {t("requiredCount", {
              count: requiredChecked,
              total: requiredItems.length,
            })}
          </Badge>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Progress Indicator */}
        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span>{t("completionProgress")}</span>
            <span>{Math.round(progressPercentage)}%</span>
          </div>
          <Progress className="h-2" value={progressPercentage} />
        </div>

        {/* Required Items */}
        <div>
          <h4 className="font-medium text-gray-900 mb-3 flex items-center space-x-2">
            <AlertCircle className="w-4 h-4 text-red-500" />
            <span>{t("requiredConfirmations")}</span>
          </h4>
          <div className="space-y-3">
            {requiredItems.map((item) => (
              <div
                key={item.id}
                className="flex items-start space-x-3 p-3 border border-gray-200 rounded-lg"
              >
                <Checkbox
                  checked={checkedItems[item.id] || false}
                  className="mt-1"
                  onCheckedChange={(checked) =>
                    { handleItemCheck(item.id, !!checked); }
                  }
                />
                <div className="flex-1">
                  <div className="flex items-center space-x-2 mb-1">
                    <Badge className="text-xs" variant="outline">
                      {item.category}
                    </Badge>
                  </div>
                  <p className="text-sm text-gray-700">{item.item}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Optional Items */}
        <div>
          <h4 className="font-medium text-gray-900 mb-3">
            {t("additionalConfirmations")}
          </h4>
          <div className="space-y-3">
            {checklistItems
              .filter((item) => !item.required)
              .map((item) => (
                <div
                  key={item.id}
                  className="flex items-start space-x-3 p-3 border border-gray-200 rounded-lg bg-gray-50"
                >
                  <Checkbox
                    checked={checkedItems[item.id] || false}
                    className="mt-1"
                    onCheckedChange={(checked) =>
                      { handleItemCheck(item.id, !!checked); }
                    }
                  />
                  <div className="flex-1">
                    <div className="flex items-center space-x-2 mb-1">
                      <Badge className="text-xs bg-white" variant="outline">
                        {item.category}
                      </Badge>
                    </div>
                    <p className="text-sm text-gray-700">{item.item}</p>
                  </div>
                </div>
              ))}
          </div>
        </div>

        {/* Completion Status */}
        <div className="pt-4 border-t border-gray-200">
          {allRequiredComplete ? (
            <div className="p-4 bg-green-50 rounded-lg border border-green-200">
              <div className="flex items-center space-x-2 text-green-800">
                <CheckCircle className="w-5 h-5" />
                <span className="font-medium">{t("readyForCompletion")}</span>
              </div>
              <p className="text-sm text-green-700 mt-1">
                {t("allRequiredConfirmed")}
              </p>
            </div>
          ) : (
            <div className="p-4 bg-yellow-50 rounded-lg border border-yellow-200">
              <div className="flex items-center space-x-2 text-yellow-800">
                <Clock className="w-5 h-5" />
                <span className="font-medium">
                  {t("itemsRemaining", {
                    count: requiredItems.length - requiredChecked,
                  })}
                </span>
              </div>
              <p className="text-sm text-yellow-700 mt-1">
                {t("pleaseConfirmItems")}
              </p>
            </div>
          )}
        </div>

        {/* Timestamp */}
        <div className="text-xs text-gray-500 text-center">
          {t("checklistInitiated", { date: new Date() })}
        </div>
      </CardContent>
    </Card>
  );
}
