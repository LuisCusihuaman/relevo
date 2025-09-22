import {
  recentActivity,
} from "@/common/constants";
import { recentActivityES } from "@/common/constants.es";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import { Bell, MessageSquare, Send, Users, X } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { ActivityFeed, type ActivityItem } from "./ActivityFeed";
import type { JSX } from "react";
import { useHandoverMessages, useCreateHandoverMessage } from "@/api";

interface CollaborationPanelProps {
  onClose: () => void;
  onNavigateToSection: (section: string) => void;
  handoverId: string;
  hideHeader?: boolean;
}

// Removed hardcoded discussion messages - now using real API data

export function CollaborationPanel({
  onClose,
  onNavigateToSection,
  handoverId,
  hideHeader = false,
}: CollaborationPanelProps): JSX.Element {
  const [newMessage, setNewMessage] = useState("");

  // Fetch handover messages
  const { data: messages, isLoading: messagesLoading, error: messagesError } = useHandoverMessages(handoverId);
  const createMessageMutation = useCreateHandoverMessage();
  const [activeTab, setActiveTab] = useState("discussion");
  const { t, i18n } = useTranslation("collaborationPanel");
  const currentRecentActivity =
    i18n.language === "es" ? recentActivityES : recentActivity;

  // Transform API messages to component format
  const transformedMessages = messages
    ? messages.map((msg) => ({
        id: parseInt(msg.id) || Math.random(),
        user: msg.userName,
        userInitials: msg.userName.split(' ').map(n => n[0]).join('').toUpperCase(),
        userColor: "bg-blue-600", // Could be derived from user ID
        role: "Physician", // Could be derived from user role if available
        message: msg.messageText,
        time: new Date(msg.createdAt).toLocaleString(),
        timestamp: new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        type: msg.messageType as "message" | "system" | "notification",
        mentions: [], // Could be parsed from message content if needed
      }))
    : []; // No fallback hardcoded data

  const handleSendMessage = (): void => {
    if (newMessage.trim() && handoverId) {
      createMessageMutation.mutate({
        handoverId,
        messageText: newMessage.trim(),
        messageType: "message",
      });
      setNewMessage("");
    }
  };

  const handleKeyPress = (event: React.KeyboardEvent): void => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      handleSendMessage();
    }
  };

  return (
    <div className="h-full flex flex-col bg-white">
      {/* Header - Only show if not hidden */}
      {!hideHeader && (
        <div className="p-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-medium text-gray-900">
              {t("header.title")}
            </h3>
            <Button
              className="h-8 w-8 p-0 hover:bg-gray-200"
              size="sm"
              variant="ghost"
              onClick={onClose}
            >
              <X className="w-4 h-4" />
            </Button>
          </div>

          {/* Real-time status */}
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-gray-600">{t("header.liveSession")}</span>
            </div>
            <span className="text-gray-500">{t("header.sessionInfo")}</span>
          </div>
        </div>
      )}

      {/* Session status bar - Always show when header is hidden */}
      {hideHeader && (
        <div className="px-4 py-3 bg-gray-50 border-b border-gray-100">
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-gray-600">{t("header.liveSession")}</span>
            </div>
            <span className="text-gray-500">{t("header.sessionInfo")}</span>
          </div>
        </div>
      )}

      {/* Tabs */}
      <Tabs
        className="flex-1 flex flex-col"
        value={activeTab}
        onValueChange={setActiveTab}
      >
        <div className="px-4 pt-3 border-b border-gray-100">
          <TabsList className="grid w-full grid-cols-2 bg-gray-100">
            <TabsTrigger
              className="text-xs data-[state=active]:bg-white"
              value="discussion"
            >
              <MessageSquare className="w-3 h-3 mr-1" />
              {t("tabs.discussion")}
            </TabsTrigger>
            <TabsTrigger
              className="text-xs data-[state=active]:bg-white"
              value="activity"
            >
              <Bell className="w-3 h-3 mr-1" />
              {t("tabs.updates")}
            </TabsTrigger>
          </TabsList>
        </div>

        {/* Discussion Tab */}
        <TabsContent className="flex-1 flex flex-col mt-0" value="discussion">
          <div className="px-4 py-3 bg-blue-50 border-b border-blue-100">
            <div className="flex items-center space-x-2">
              <Users className="w-4 h-4 text-blue-600" />
              <div className="flex-1">
                <h4 className="text-sm font-medium text-blue-900">
                  {t("discussionTab.title")}
                </h4>
                <p className="text-xs text-blue-700">
                  {t("discussionTab.subtitle")}
                </p>
              </div>
            </div>
          </div>

          {/* Messages */}
          <ScrollArea className="flex-1 px-4">
            <div className="space-y-4 py-4">
              {messagesLoading ? (
                <div className="text-center text-gray-500 py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-2"></div>
                  {t("loadingMessages")}
                </div>
              ) : messagesError ? (
                <div className="text-center text-red-500 py-8">
                  {t("errorLoadingMessages")}
                </div>
              ) : transformedMessages.length === 0 ? (
                <div className="text-center text-gray-500 py-8">
                  {t("noMessagesYet")}
                </div>
              ) : (
                transformedMessages.map((message) => (
                <div key={message.id} className="space-y-2">
                  <div className="flex items-start space-x-3">
                    <Avatar className="w-8 h-8 flex-shrink-0">
                      <AvatarFallback
                        className={`${message.userColor} text-white text-xs`}
                      >
                        {message.userInitials}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center space-x-2 mb-1">
                        <span className="text-sm font-medium text-gray-900">
                          {t(`discussion.${message.user.replace("Dr. ", "user").toLowerCase()}.user`)}
                        </span>
                        <span className="text-xs text-gray-500">
                          {t(`discussion.${message.user.replace("Dr. ", "user").toLowerCase()}.role`)}
                        </span>
                        <span className="text-xs text-gray-400">
                          {t(`discussion.${message.user.replace("Dr. ", "user").toLowerCase()}.time`)}
                        </span>
                      </div>
                      <div className="text-sm text-gray-700 leading-relaxed">
                        {t(`discussion.${message.user.replace("Dr. ", "user").toLowerCase()}.message`)}
                      </div>
                    </div>
                  </div>
                </div>
              ))
              )}
            </div>
          </ScrollArea>

          {/* Message Input */}
          <div className="p-4 border-t border-gray-200 bg-gray-50">
            <div className="space-y-3">
              <div className="relative">
                <Textarea
                  className="bg-white min-h-[5rem] pr-20"
                  placeholder={t("messageInput.placeholder")}
                  rows={3}
                  value={newMessage}
                  onChange={(event) => { setNewMessage(event.target.value); }}
                  onKeyPress={handleKeyPress}
                />
                <Button
                  className="absolute bottom-2 right-2"
                  size="sm"
                  onClick={handleSendMessage}
                >
                  <Send className="w-4 h-4 mr-2" />
                  {t("messageInput.send")}
                </Button>
              </div>
            </div>
          </div>
        </TabsContent>

        {/* Activity Tab */}
        <TabsContent className="flex-1 flex flex-col mt-0" value="activity">
          <ActivityFeed
            items={currentRecentActivity as Array<ActivityItem>}
            onNavigateToSection={onNavigateToSection}
          />
        </TabsContent>
      </Tabs>
    </div>
  );
}
