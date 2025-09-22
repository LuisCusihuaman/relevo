import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Textarea } from "@/components/ui/textarea";
import {
  Activity,
  AlertTriangle,
  CheckSquare,
  Edit,
  FileText,
  MessageSquare,
  Save,
  Star,
  Target,
  X,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

interface Patient {
  name: string;
  room: string;
}

interface QuickActionsProps {
  patient: Patient;
  onClose: () => void;
}

export function QuickActions({ patient, onClose }: QuickActionsProps) {
  const { t } = useTranslation("quickActions");
  const [activeForm, setActiveForm] = useState<string | null>(null);
  const [quickNote, setQuickNote] = useState("");
  const [targetSection, setTargetSection] = useState("patient-summary");

  // I-PASS specific templates
  const ipassTemplates = [
    {
      id: "illness-update",
      title: t("ipassTemplates.illness.title"),
      icon: Activity,
      section: "illness",
      template: t("ipassTemplates.illness.template"),
      color: "text-blue-600",
    },
    {
      id: "patient-summary",
      title: t("ipassTemplates.patient.title"),
      icon: FileText,
      section: "patient",
      template: t("ipassTemplates.patient.template"),
      color: "text-green-600",
    },
    {
      id: "action-item",
      title: t("ipassTemplates.action.title"),
      icon: Target,
      section: "actions",
      template: t("ipassTemplates.action.template"),
      color: "text-orange-600",
    },
    {
      id: "situation-awareness",
      title: t("ipassTemplates.situation.title"),
      icon: AlertTriangle,
      section: "awareness",
      template: t("ipassTemplates.situation.template"),
      color: "text-purple-600",
    },
    {
      id: "synthesis-note",
      title: t("ipassTemplates.synthesis.title"),
      icon: MessageSquare,
      section: "synthesis",
      template: t("ipassTemplates.synthesis.template"),
      color: "text-red-600",
    },
  ];

  // I-PASS section tracking
  const ipassProgress = {
    illness: {
      complete: true,
      label: t("ipassProgress.illness.label"),
      icon: Activity,
    },
    patient: {
      complete: true,
      label: t("ipassProgress.patient.label"),
      icon: FileText,
    },
    actions: {
      complete: false,
      label: t("ipassProgress.action.label"),
      icon: Target,
    },
    awareness: {
      complete: false,
      label: t("ipassProgress.awareness.label"),
      icon: AlertTriangle,
    },
    synthesis: {
      complete: false,
      label: t("ipassProgress.synthesis.label"),
      icon: MessageSquare,
    },
  };

  const handleQuickSave = (content: string, section: string) => {
    // In real app, this would save to the appropriate I-PASS section
    console.log(`Saving to ${section}: ${content}`);
    setQuickNote("");
    setActiveForm(null);
    // Show success notification
  };

  const insertTemplate = (template: string, section: string) => {
    setQuickNote(template);
    setTargetSection(section);
    setActiveForm("note");
  };

  const completedSections = Object.values(ipassProgress).filter(
    (s) => s.complete,
  ).length;
  const totalSections = Object.keys(ipassProgress).length;

  return (
    <div className="h-full flex flex-col bg-white">
      {/* Header */}
      <div className="p-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-medium text-gray-900">{t("title")}</h3>
            <p className="text-sm text-gray-600">
              {t("patientInfo", { name: patient.name, room: patient.room })}
            </p>
          </div>
          <Button
            className="h-8 w-8 p-0"
            size="sm"
            variant="ghost"
            onClick={onClose}
          >
            <X className="w-4 h-4" />
          </Button>
        </div>
      </div>

      <ScrollArea className="flex-1 p-4">
        <div className="space-y-6">
          {/* I-PASS Progress Overview */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h4 className="font-medium text-gray-900">
                {t("progressOverview.title")}
              </h4>
              <Badge className="text-xs" variant="outline">
                {t("progressOverview.sectionsCompleted", {
                  completed: completedSections,
                  total: totalSections,
                })}
              </Badge>
            </div>

            <div className="space-y-2">
              {Object.entries(ipassProgress).map(([key, section]) => {
                const IconComponent = section.icon;
                return (
                  <div
                    key={key}
                    className={`flex items-center justify-between p-2 rounded-lg border ${
                      section.complete
                        ? "border-green-200 bg-green-50"
                        : "border-gray-200 bg-gray-50"
                    }`}
                  >
                    <div className="flex items-center space-x-2">
                      <IconComponent
                        className={`w-4 h-4 ${
                          section.complete ? "text-green-600" : "text-gray-500"
                        }`}
                      />
                      <span className="text-sm">{section.label}</span>
                    </div>
                    {section.complete ? (
                      <CheckSquare className="w-4 h-4 text-green-600" />
                    ) : (
                      <div className="w-4 h-4 border-2 border-gray-300 rounded"></div>
                    )}
                  </div>
                );
              })}
            </div>
          </div>

          <Separator />

          {/* Quick Note with I-PASS Section Targeting */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h4 className="font-medium text-gray-900">{t("addToIPass")}</h4>
              <Button
                className="text-xs"
                size="sm"
                variant="ghost"
                onClick={() =>
                  { setActiveForm(activeForm === "note" ? null : "note"); }
                }
              >
                <Edit className="w-3 h-3 mr-1" />
                {activeForm === "note" ? t("cancel") : t("add")}
              </Button>
            </div>

            {activeForm === "note" && (
              <div className="space-y-3">
                <Select value={targetSection} onValueChange={setTargetSection}>
                  <SelectTrigger className="text-sm">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="illness">
                      {t("ipassSections.illness")}
                    </SelectItem>
                    <SelectItem value="patient">
                      {t("ipassSections.patient")}
                    </SelectItem>
                    <SelectItem value="actions">
                      {t("ipassSections.action")}
                    </SelectItem>
                    <SelectItem value="awareness">
                      {t("ipassSections.situation")}
                    </SelectItem>
                    <SelectItem value="synthesis">
                      {t("ipassSections.synthesis")}
                    </SelectItem>
                  </SelectContent>
                </Select>

                <Textarea
                  className="min-h-[80px] text-sm"
                  placeholder={t("notePlaceholder")}
                  value={quickNote}
                  onChange={(e) => { setQuickNote(e.target.value); }}
                />

                <div className="flex space-x-2">
                  <Button
                    className="text-xs flex-1"
                    disabled={!quickNote.trim()}
                    size="sm"
                    onClick={() => { handleQuickSave(quickNote, targetSection); }}
                  >
                    <Save className="w-3 h-3 mr-1" />
                    {t("addTo", {
                      section:
                        targetSection.charAt(0).toUpperCase() +
                        targetSection.slice(1),
                    })}
                  </Button>
                  <Button
                    className="text-xs"
                    size="sm"
                    variant="outline"
                    onClick={() => { setQuickNote(""); }}
                  >
                    {t("clear")}
                  </Button>
                </div>
              </div>
            )}
          </div>

          <Separator />

          {/* I-PASS Templates */}
          <div className="space-y-3">
            <h4 className="font-medium text-gray-900">
              {t("quickTemplates")}
            </h4>
            <div className="space-y-2">
              {ipassTemplates.map((template) => {
                const IconComponent = template.icon;
                return (
                  <Button
                    key={template.id}
                    className="justify-start text-xs h-auto py-3 w-full"
                    size="sm"
                    variant="outline"
                    onClick={() =>
                      { insertTemplate(template.template, template.section); }
                    }
                  >
                    <div className="flex items-center space-x-2 w-full">
                      <IconComponent
                        className={`w-4 h-4 flex-shrink-0 ${template.color}`}
                      />
                      <div className="flex-1 text-left">
                        <div className="font-medium">{template.title}</div>
                        <div className="text-xs text-gray-500 mt-1">
                          {t("passSection", {
                            letter: template.section.charAt(0).toUpperCase(),
                          })}
                        </div>
                      </div>
                    </div>
                  </Button>
                );
              })}
            </div>
          </div>
        </div>
      </ScrollArea>

      {/* Footer */}
      <div className="p-4 border-t border-gray-200">
        <div className="flex items-center justify-between text-xs text-gray-500">
          <span>
            {t("footer.progress", {
              progress: Math.round((completedSections / totalSections) * 100),
            })}
          </span>
          <Button className="text-xs" size="sm" variant="ghost">
            <Star className="w-3 h-3 mr-1" />
            {t("footer.saveTemplate")}
          </Button>
        </div>
      </div>
    </div>
  );
}
