import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import {
  Calendar,
  Edit3,
  FileText,
  MessageSquare,
  Plus,
  User,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

interface JustificationProps {
  collaborators: Array<{
    id: number;
    name: string;
    initials: string;
    color: string;
    section: string;
  }>;
}

export function Justification({ collaborators }: JustificationProps) {
  const { t } = useTranslation("justification");
  const [isAddingEntry, setIsAddingEntry] = useState(false);
  const [newEntry, setNewEntry] = useState("");

  const justificationEntries = [
    {
      id: 1,
      timestamp: "14:15",
      author: "authors.martinez",
      initials: "AM",
      content: "entries.content1",
      priority: "high",
      isBeingEdited: true,
      editedBy: "authors.johnson",
    },
    {
      id: 2,
      timestamp: "13:45",
      author: "authors.johnson",
      initials: "DJ",
      content: "entries.content2",
      priority: "medium",
      isBeingEdited: false,
    },
  ];

  const activeEditor = collaborators.find((c) => c.section === "justification");

  const handleAddEntry = () => {
    if (newEntry.trim()) {
      // In real app, this would add to the shared document
      setNewEntry("");
      setIsAddingEntry(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <FileText className="w-5 h-5 text-blue-600" />
            <span>{t("title")}</span>
            {activeEditor && (
              <div className="flex items-center space-x-2 ml-4">
                <Avatar className="w-5 h-5">
                  <AvatarFallback
                    className={`${activeEditor.color} text-white text-xs`}
                  >
                    {activeEditor.initials}
                  </AvatarFallback>
                </Avatar>
                <span className="text-sm text-gray-600">
                  {t("isEditing", { name: activeEditor.name })}
                </span>
                <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
              </div>
            )}
          </div>
          <Button
            className="flex items-center space-x-1"
            size="sm"
            variant="outline"
            onClick={() => { setIsAddingEntry(!isAddingEntry); }}
          >
            <Plus className="w-3 h-3" />
            <span>{t("addEntry")}</span>
          </Button>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Add New Entry */}
        {isAddingEntry && (
          <div className="p-4 border-2 border-blue-200 rounded-lg bg-blue-50">
            <h4 className="font-medium text-gray-900 mb-3">
              {t("newEntryTitle")}
            </h4>
            <Textarea
              className="min-h-[100px] mb-3"
              placeholder={t("newEntryPlaceholder")}
              value={newEntry}
              onChange={(e) => { setNewEntry(e.target.value); }}
            />
            <div className="flex space-x-2">
              <Button size="sm" onClick={handleAddEntry}>
                {t("addEntry")}
              </Button>
              <Button
                size="sm"
                variant="outline"
                onClick={() => { setIsAddingEntry(false); }}
              >
                {t("cancel")}
              </Button>
            </div>
          </div>
        )}

        {/* Existing Entries */}
        <div className="space-y-4">
          {justificationEntries.map((entry) => (
            <div
              key={entry.id}
              className={`border rounded-lg p-4 space-y-3 ${
                entry.isBeingEdited
                  ? "border-blue-300 bg-blue-50"
                  : "border-gray-200"
              }`}
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2 text-sm text-gray-600">
                  <Calendar className="w-4 h-4" />
                  <span>{entry.timestamp}</span>
                  <User className="w-4 h-4 ml-2" />
                  <span>{t(entry.author)}</span>
                </div>
                <div className="flex items-center space-x-2">
                  <Badge
                    className="text-xs"
                    variant={
                      entry.priority === "high" ? "destructive" : "secondary"
                    }
                  >
                    {entry.priority.toUpperCase()}
                  </Badge>
                  {entry.isBeingEdited && entry.editedBy && (
                    <div className="flex items-center space-x-1 text-xs text-blue-600">
                      <Edit3 className="w-3 h-3" />
                      <span>{t("editingBy", { name: t(entry.editedBy) })}</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="relative">
                <p className="text-sm leading-relaxed">{t(entry.content)}</p>
                {entry.isBeingEdited && (
                  <div className="absolute inset-0 bg-blue-100 bg-opacity-30 border border-blue-300 rounded pointer-events-none"></div>
                )}
              </div>

              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <Avatar className="w-6 h-6">
                    <AvatarFallback className="bg-gray-400 text-white text-xs">
                      {entry.initials}
                    </AvatarFallback>
                  </Avatar>
                  <span className="text-xs text-gray-500">{t(entry.author)}</span>
                </div>
                <Button className="text-xs" size="sm" variant="ghost">
                  <MessageSquare className="w-3 h-3 mr-1" />
                  {t("comment")}
                </Button>
              </div>
            </div>
          ))}
        </div>

        {/* Collaboration Footer */}
        <div className="flex justify-between items-center text-sm text-gray-500 pt-4 border-t border-gray-200">
          <div className="flex items-center space-x-2">
            <span>{t("lastUpdated", { time: "14:15 PMT" })}</span>
            <span>•</span>
            <span>{t("entriesTotal", { count: justificationEntries.length })}</span>
          </div>
          <div className="flex items-center space-x-2">
            <span>{t("sharedWithTeam")}</span>
            <div className="flex -space-x-1">
              {collaborators.slice(0, 3).map((collaborator) => (
                <Avatar
                  key={collaborator.id}
                  className="w-5 h-5 border border-white"
                >
                  <AvatarFallback
                    className={`${collaborator.color} text-white text-xs`}
                  >
                    {collaborator.initials}
                  </AvatarFallback>
                </Avatar>
              ))}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
