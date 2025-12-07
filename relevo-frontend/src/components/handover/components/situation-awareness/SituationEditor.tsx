import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Textarea } from "@/components/ui/textarea";
import { Activity, Edit, Users } from "lucide-react";
import { type ChangeEvent, type KeyboardEvent, useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";

interface SituationEditorProps {
  content: string;
  isEditing: boolean;
  onEditChange: (isEditing: boolean) => void;
  onChange: (content: string) => void;
  autoSaveStatus: "saved" | "saving" | "error";
  fullscreenMode?: boolean;
  hideControls?: boolean;
  canEdit: boolean;
  onEnterEdit?: () => void;
}

export function SituationEditor(props: SituationEditorProps): JSX.Element {
  const {
    content,
    isEditing,
    onEditChange,
    onChange,
    autoSaveStatus,
    fullscreenMode = false,
    hideControls = false,
    canEdit,
    onEnterEdit,
  } = props;
  const { t } = useTranslation("situationAwareness");
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Focus textarea when entering edit mode
  useEffect(() => {
    if (isEditing && textareaRef.current) {
      textareaRef.current.focus();
    }
  }, [isEditing]);

  const contentHeight = fullscreenMode ? "min-h-[60vh]" : "h-80";

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>): void => {
    if ((event.key === "Enter" || event.key === " ") && canEdit && !isEditing) {
      event.preventDefault();
      onEnterEdit?.();
    }
  };

  const onTextareaChange = (event: ChangeEvent<HTMLTextAreaElement>): void => {
    onChange(event.target.value);
  };

  if (isEditing) {
    return (
      /* Editing Mode - No top border radius */
      <div className="relative">
        <div
          className={`bg-white border-2 border-blue-200 ${fullscreenMode ? "rounded-lg" : "rounded-t-none rounded-b-none"}`}
        >
          {/* Enhanced Editor Header with top rounded corners */}
          <div
            className={`flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-blue-25/50 ${fullscreenMode ? "rounded-t-lg" : "rounded-t-lg"}`}
          >
            <div className="flex items-center space-x-3">
              <Edit className="w-5 h-5 text-blue-600" />
              <h4 className="text-lg font-medium text-blue-800">
                {fullscreenMode
                  ? t("editor.fullscreenTitle")
                  : t("editor.title")}
              </h4>
            </div>
            <div className="flex items-center space-x-3">
              <div className="flex items-center space-x-1">
                <div
                  className={`w-2 h-2 rounded-full ${
                    autoSaveStatus === "saved"
                      ? "bg-green-500"
                      : autoSaveStatus === "saving"
                        ? "bg-amber-500 animate-pulse"
                        : "bg-red-500"
                  }`}
                ></div>
                <span className="text-sm text-blue-600">
                  {autoSaveStatus === "saved"
                    ? t("autoSave.saved")
                    : autoSaveStatus === "saving"
                      ? t("autoSave.saving")
                      : t("autoSave.error")}
                </span>
              </div>
              {/* ONLY SHOW DONE BUTTON IF NOT HIDING CONTROLS */}
              {!hideControls && (
                <Button
                  className="text-xs text-blue-600 hover:bg-blue-100 h-7 px-2"
                  size="sm"
                  variant="ghost"
                  onClick={() => { onEditChange(false); }}
                >
                  {t("done")}
                </Button>
              )}
            </div>
          </div>

          {/* Fixed Height Document Content Area - no border radius */}
          <div className={`relative ${contentHeight}`}>
            <ScrollArea className="h-full">
              <div className="p-6">
                <Textarea
                  ref={textareaRef}
                  className={`w-full h-full ${fullscreenMode ? "min-h-[60vh]" : "min-h-[400px]"} border-0 bg-transparent p-4 resize-none text-gray-900 leading-relaxed placeholder:text-gray-400 focus:outline-none focus:ring-0 focus:ring-offset-0 focus-visible:ring-0 focus-visible:ring-offset-0 rounded-none`}
                  placeholder={String(t("editor.placeholder"))}
                  value={content}
                  style={{
                    fontFamily: "system-ui, -apple-system, sans-serif",
                    fontSize: fullscreenMode ? "16px" : "14px",
                    lineHeight: "1.6",
                    background: "transparent !important",
                  }}
                  onChange={onTextareaChange}
                />
              </div>
            </ScrollArea>
          </div>

          {/* Auto-Save Status Footer - no bottom rounded corners */}
          <div
            className={`flex items-center justify-between px-4 py-2 border-t border-gray-100 bg-gray-25/30 ${fullscreenMode ? "rounded-b-lg" : ""}`}
          >
            <div className="flex items-center space-x-3 text-xs text-gray-500">
              <span>
                {content.split("\n").length} {t("editor.lines")}
              </span>
              <span>
                {content.split(" ").length} {t("editor.words")}
              </span>
              {!hideControls && <span>{t("editor.autosaving")}</span>}
              {hideControls && (
                <span>{t("editor.useFullscreenControls")}</span>
              )}
            </div>
            <span className="text-xs text-gray-500">
              {t("editor.peopleEditing", { count: 3 })}
            </span>
          </div>
        </div>
      </div>
    );
  }

  return (
    /* View Mode - No top border radius */
    <div
      className="relative group"
      role={canEdit ? "button" : undefined}
      tabIndex={canEdit ? 0 : undefined}
      aria-label={
        canEdit ? t("view.editAriaLabel") : undefined
      }
      onClick={onEnterEdit}
      onKeyDown={handleKeyDown}
    >
      <div
        className={`bg-white ${fullscreenMode ? "rounded-lg" : "rounded-t-none rounded-b-none"} transition-all duration-200 ${canEdit ? "cursor-pointer" : ""}`}
      >
        {/* Enhanced Header with top rounded corners */}
        <div
          className={`flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-blue-25/50 ${fullscreenMode ? "rounded-t-lg" : "rounded-t-lg"}`}
        >
          <div className="flex items-center space-x-3">
            <Activity className="w-5 h-5 text-blue-600" />
            <div className="flex items-center space-x-3">
              <h4 className="text-lg font-medium text-blue-700">
                {fullscreenMode
                  ? t("view.fullscreenTitle")
                  : t("currentSituation.title")}
              </h4>
              <Badge
                className="text-xs bg-blue-50 text-blue-700 border-blue-200"
                variant="outline"
              >
                <Users className="w-3 h-3 mr-1" />
                {t("view.allCanEdit")}
              </Badge>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-2 h-2 bg-green-500 rounded-full"></div>
            <span className="text-sm text-gray-500">{t("view.active")}</span>
          </div>
        </div>

        {/* Fixed Height Document Content - no border radius */}
        <div className={`relative ${contentHeight}`}>
          <ScrollArea className="h-full">
            <div className="p-6">
              <div
                className="text-gray-900 leading-relaxed whitespace-pre-line"
                style={{
                  fontFamily: "system-ui, -apple-system, sans-serif",
                  fontSize: fullscreenMode ? "16px" : "14px",
                  lineHeight: "1.6",
                }}
              >
                {content}
              </div>
            </div>
          </ScrollArea>
        </div>

        {/* Document Footer - no bottom rounded corners */}
        <div
          className={`flex items-center justify-between px-4 py-2 border-t border-gray-100 bg-gray-25/30 ${fullscreenMode ? "rounded-b-lg" : ""}`}
        >
          <div className="flex items-center space-x-3 text-xs text-gray-500">
            <span>
              {content.split("\n").length} {t("editor.lines")}
            </span>
            <span>
              {content.split(" ").length} {t("editor.words")}
            </span>
            <span>{t("view.lastUpdatedBy", { user: "Dr. Rodriguez" })}</span>
          </div>
          {canEdit && (
            <div className="opacity-0 group-hover:opacity-100 transition-opacity">
              <div className="flex items-center space-x-1 text-xs text-gray-500">
                <Edit className="w-3 h-3" />
                <span>{t("view.clickToEdit")}</span>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
