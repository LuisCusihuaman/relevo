import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Separator } from "@/components/ui/separator";
import { Textarea } from "@/components/ui/textarea";
import {
  CheckSquare,
  Clock,
  History,
  Plus,
  Send,
  Trash2,
  X,
} from "lucide-react";
import type React from "react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

interface Collaborator {
  id: number;
  name: string;
  initials: string;
  color: string;
  role: string;
}

interface ActionItem {
  id: number;
  task: string;
  priority: "low" | "medium" | "high";
  dueTime?: string;
  completed: boolean;
  submittedBy: string;
  submittedTime: string;
  submittedDate: string;
  shift: string;
}

interface ActionListProps {
  expanded?: boolean;
  collaborators?: Array<Collaborator>;
  onOpenThread?: (section: string) => void;
  focusMode?: boolean;
  compact?: boolean;
  currentUser?: {
    name: string;
    initials: string;
    role: string;
  };
  assignedPhysician?: {
    name: string;
    initials: string;
    role: string;
  };
}

export function ActionList({
  expanded: _expanded = false,
  collaborators: _collaborators = [],
  onOpenThread: _onOpenThread,
  focusMode = false,
  compact = false,
  currentUser = { name: "Dr. Johnson", initials: "DJ", role: "Day Attending" },
  assignedPhysician = {
    name: "Dr. Johnson",
    initials: "DJ",
    role: "Day Attending",
  },
}: ActionListProps) {
  const { t } = useTranslation("actionList");
  // Action items from multiple shifts - persistent until completed or handover ends
  const [actionItems, setActionItems] = useState<Array<ActionItem>>([
    // Previous Night Shift tasks (still pending)
    {
      id: 1,
      task: t("actionItems.followUpCulture.task"),
      priority: "high",
      dueTime: t("actionItems.followUpCulture.dueTime"),
      completed: false,
      submittedBy: t("doctors.park"),
      submittedTime: "02:15",
      submittedDate: t("time.lastNight"),
      shift: t("shifts.nightToDay"),
    },
    {
      id: 2,
      task: t("actionItems.cardiologyConsult.task"),
      priority: "medium",
      dueTime: t("time.today"),
      completed: false,
      submittedBy: t("doctors.kim"),
      submittedTime: "23:45",
      submittedDate: t("time.lastNight"),
      shift: t("shifts.nightToDay"),
    },
    // Current Day Shift tasks
    {
      id: 3,
      task: t("actionItems.dischargePlanning.task"),
      priority: "medium",
      completed: true,
      submittedBy: t("doctors.johnson"),
      submittedTime: "09:30",
      submittedDate: t("time.today"),
      shift: t("shifts.dayToEvening"),
    },
    {
      id: 4,
      task: t("actionItems.reviewXray.task"),
      priority: "high",
      dueTime: t("actionItems.reviewXray.dueTime"),
      completed: false,
      submittedBy: t("doctors.martinez"),
      submittedTime: "11:15",
      submittedDate: t("time.today"),
      shift: t("shifts.dayToEvening"),
    },
    {
      id: 5,
      task: t("actionItems.monitorUrine.task"),
      priority: "high",
      dueTime: t("actionItems.monitorUrine.dueTime"),
      completed: false,
      submittedBy: t("nurses.clara"),
      submittedTime: "15:30",
      submittedDate: t("time.today"),
      shift: t("shifts.dayToEvening"),
    },
  ]);

  const [showNewTaskForm, setShowNewTaskForm] = useState(false);
  const [newTask, setNewTask] = useState({
    task: "",
    priority: "medium" as "low" | "medium" | "high",
    dueTime: "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Check if current user can delete tasks (only assigned physician for current shift)
  const canDeleteTasks = currentUser.name === assignedPhysician.name;

  // Group tasks by status only - no urgent separation
  const pendingTasks = actionItems.filter((item) => !item.completed);
  const completedTasks = actionItems.filter((item) => item.completed);

  // Submit new task
  const handleSubmitTask = () => {
    if (!newTask.task.trim()) return;

    setIsSubmitting(true);

    // Simulate API call
    setTimeout(() => {
      const task: ActionItem = {
        id: Date.now(),
        task: newTask.task.trim(),
        priority: newTask.priority,
        dueTime: newTask.dueTime.trim() || undefined,
        completed: false,
        submittedBy: currentUser.name,
        submittedTime: new Date().toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
        submittedDate: t("time.today"),
        shift: t("shifts.dayToEvening"),
      };

      setActionItems((previous) => [...previous, task]);
      setNewTask({ task: "", priority: "medium", dueTime: "" });
      setShowNewTaskForm(false);
      setIsSubmitting(false);
    }, 500);
  };

  // Toggle task completion
  const handleToggleComplete = (taskId: number) => {
    setActionItems((previous) =>
      previous.map((item) =>
        item.id === taskId ? { ...item, completed: !item.completed } : item,
      ),
    );
  };

  // Delete task (only current shift tasks by assigned physician)
  const handleDeleteTask = (taskId: number) => {
    const task = actionItems.find((t) => t.id === taskId);
    if (!canDeleteTasks || task?.shift !== t("shifts.dayToEvening")) return;

    setActionItems((previous) => previous.filter((item) => item.id !== taskId));
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && (e.metaKey || e.ctrlKey)) {
      e.preventDefault();
      handleSubmitTask();
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "high":
        return "text-red-600 border-red-200 bg-red-50";
      case "medium":
        return "text-amber-600 border-amber-200 bg-amber-50";
      case "low":
        return "text-emerald-600 border-emerald-200 bg-emerald-50";
      default:
        return "text-gray-600 border-gray-200 bg-gray-50";
    }
  };

  // Clean task card component
  const TaskCard = ({ task }: { task: ActionItem }) => (
    <div
      className={`p-4 rounded-lg border transition-all hover:shadow-sm group ${
        task.completed
          ? "border-gray-200 bg-gray-50"
          : "border-gray-200 bg-white hover:border-gray-300"
      }`}
    >
      <div className="space-y-3">
        {/* Task Header with Checkbox */}
        <div className="flex items-start justify-between">
          <div className="flex items-start space-x-3 flex-1 min-w-0">
            <Checkbox
              checked={task.completed}
              className={`mt-1 ${task.completed ? "bg-gray-600 border-gray-600" : ""}`}
              disabled={focusMode}
              onCheckedChange={
                focusMode ? undefined : () => { handleToggleComplete(task.id); }
              }
            />
            <div className="flex-1 min-w-0">
              <div className="flex items-center space-x-2 mb-2">
                <div
                  className={`text-xs px-2 py-1 rounded border font-medium ${getPriorityColor(task.priority)}`}
                >
                  {t(`priorities.${task.priority}`)}
                </div>
              </div>
              <p
                className={`text-sm leading-relaxed ${
                  task.completed
                    ? "line-through text-gray-500"
                    : "text-gray-900"
                }`}
              >
                {task.task}
              </p>
            </div>
          </div>

          {/* Delete button - Only for current shift tasks by assigned physician */}
          {!focusMode &&
            canDeleteTasks &&
            task.shift === t("shifts.dayToEvening") &&
            !task.completed && (
              <Button
                className="opacity-0 group-hover:opacity-100 transition-opacity text-red-600 hover:text-red-700 hover:bg-red-50 h-6 w-6 p-0"
                size="sm"
                variant="ghost"
                onClick={() => { handleDeleteTask(task.id); }}
              >
                <Trash2 className="w-3 h-3" />
              </Button>
            )}
        </div>

        {/* Task Details */}
        <div
          className={`flex items-center justify-between text-xs pt-2 border-t ${
            task.completed
              ? "border-gray-100 text-gray-400"
              : "border-gray-100 text-gray-500"
          }`}
        >
          <div className="flex items-center space-x-3">
            {task.dueTime && (
              <div className="flex items-center space-x-1">
                <Clock className="w-3 h-3" />
                <span>{task.dueTime}</span>
              </div>
            )}
          </div>
          <span className={task.completed ? "text-gray-400" : "text-gray-500"}>
            {task.submittedBy} • {task.submittedTime}
          </span>
        </div>
      </div>
    </div>
  );

  if (compact) {
    // Compact version for sidebar
    return (
      <div className="space-y-4">
        {/* Quick Stats */}
        <div className="flex items-center justify-between">
          <h4 className="font-medium text-gray-900">{t("title")}</h4>
          <div className="flex items-center space-x-2">
            <Badge className="text-xs" variant="outline">
              {pendingTasks.length} {t("pending").toLowerCase()}
            </Badge>
            {completedTasks.length > 0 && (
              <Badge className="bg-gray-100 text-gray-600 border-gray-200 text-xs">
                {completedTasks.length} {t("done").toLowerCase()}
              </Badge>
            )}
          </div>
        </div>

        {/* Pending Tasks - Limited */}
        {pendingTasks.length > 0 && (
          <div className="space-y-2">
            <h5 className="text-sm font-medium text-gray-700">{t("pending")}</h5>
            {pendingTasks.slice(0, 3).map((task) => (
              <TaskCard key={task.id} task={task} />
            ))}
            {pendingTasks.length > 3 && (
              <p className="text-xs text-gray-500 text-center py-2">
                +{pendingTasks.length - 3} {t("moreTasks")}
              </p>
            )}
          </div>
        )}

        {/* Completed Tasks - Limited in Compact Mode */}
        {completedTasks.length > 0 && (
          <div className="space-y-2">
            <h5 className="text-sm font-medium text-gray-600">{t("completed")}</h5>
            {completedTasks.slice(0, 2).map((task) => (
              <TaskCard key={task.id} task={task} />
            ))}
            {completedTasks.length > 2 && (
              <p className="text-xs text-gray-500 text-center py-2">
                +{completedTasks.length - 2} {t("moreCompleted")}
              </p>
            )}
          </div>
        )}

        {/* Add New Task - Compact Form */}
        {!focusMode && !showNewTaskForm ? (
          <Button
            className="w-full text-xs border-gray-200 hover:bg-gray-50"
            size="sm"
            variant="outline"
            onClick={() => { setShowNewTaskForm(true); }}
          >
            <Plus className="w-3 h-3 mr-1" />
            {t("addTask")}
          </Button>
        ) : (
          !focusMode && (
            <div className="p-3 border border-gray-200 rounded-lg bg-gray-25">
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <h5 className="font-medium text-gray-900 text-sm">
                    {t("newTask")}
                  </h5>
                  <Button
                    className="h-5 w-5 p-0"
                    disabled={isSubmitting}
                    size="sm"
                    variant="ghost"
                    onClick={() => { setShowNewTaskForm(false); }}
                  >
                    <X className="w-3 h-3" />
                  </Button>
                </div>

                <div className="space-y-2">
                  <div>
                    <Textarea
                      className="min-h-[50px] text-sm border-gray-300 focus:border-blue-400 focus:ring-blue-100 bg-white resize-none"
                      disabled={isSubmitting}
                      placeholder={t("describeTask")}
                      value={newTask.task}
                      onKeyDown={handleKeyDown}
                      onChange={(e) =>
                        { setNewTask({ ...newTask, task: e.target.value }); }
                      }
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <select
                        className="w-full p-1.5 text-xs border border-gray-300 rounded bg-white focus:border-blue-400 focus:ring-blue-100"
                        disabled={isSubmitting}
                        value={newTask.priority}
                        onChange={(e) =>
                          { setNewTask({
                            ...newTask,
                            priority: e.target.value as
                              | "low"
                              | "medium"
                              | "high",
                          }); }
                        }
                      >
                        <option value="low">{t("low")}</option>
                        <option value="medium">{t("medium")}</option>
                        <option value="high">{t("high")}</option>
                      </select>
                    </div>

                    <div>
                      <input
                        className="w-full p-1.5 text-xs border border-gray-300 rounded bg-white focus:border-blue-400 focus:ring-blue-100"
                        disabled={isSubmitting}
                        placeholder={t("dueTime")}
                        type="text"
                        value={newTask.dueTime}
                        onChange={(e) =>
                          { setNewTask({ ...newTask, dueTime: e.target.value }); }
                        }
                      />
                    </div>
                  </div>
                </div>

                <div className="flex justify-end space-x-1">
                  <Button
                    className="text-xs px-2 py-1 h-7 border-gray-300 hover:bg-gray-50"
                    disabled={isSubmitting}
                    size="sm"
                    variant="outline"
                    onClick={() => { setShowNewTaskForm(false); }}
                  >
                    {t("cancel")}
                  </Button>
                  <Button
                    className="text-xs px-2 py-1 h-7 bg-blue-600 hover:bg-blue-700 text-white"
                    disabled={!newTask.task.trim() || isSubmitting}
                    size="sm"
                    onClick={handleSubmitTask}
                  >
                    {isSubmitting ? (
                      <>
                        <div className="w-2 h-2 border border-white border-t-transparent rounded-full animate-spin mr-1"></div>
                        {t("adding")}...
                      </>
                    ) : (
                      <>
                        <Send className="w-2 h-2 mr-1" />
                        {t("add")}
                      </>
                    )}
                  </Button>
                </div>
              </div>
            </div>
          )
        )}
      </div>
    );
  }

  // Full version - No Card container, direct content
  return (
    <div className="space-y-6">
      {/* Status Header - Clean and Simple */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <Badge className="bg-blue-50 text-blue-700 border border-blue-200 text-xs">
            {pendingTasks.length} {t("pending").toLowerCase()}
          </Badge>
          {completedTasks.length > 0 && (
            <Badge className="bg-gray-100 text-gray-600 border border-gray-200 text-xs">
              {completedTasks.length} {t("done").toLowerCase()}
            </Badge>
          )}
        </div>
      </div>

      {/* Cross-Shift Persistence Notice */}
      <div className="p-3 bg-blue-25 border border-blue-200 rounded-lg">
        <div className="flex items-center space-x-2 text-blue-800 text-sm">
          <History className="w-4 h-4" />
          <span className="font-medium">{t("tasksPersist")}</span>
        </div>
        <p className="text-blue-700 text-xs mt-1">
          {t("currentShift")}: {t("shifts.dayToEvening")}
        </p>
      </div>

      {/* Pending Tasks */}
      {pendingTasks.length > 0 && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <CheckSquare className="w-4 h-4 text-gray-600" />
              <h4 className="font-medium text-gray-900">{t("pendingTasks")}</h4>
            </div>
            <Badge className="text-xs" variant="outline">
              {pendingTasks.length} {t("active")}
            </Badge>
          </div>
          <div className="space-y-3">
            {pendingTasks.map((task) => (
              <TaskCard key={task.id} task={task} />
            ))}
          </div>
        </div>
      )}

      {/* Completed Tasks */}
      {completedTasks.length > 0 && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <CheckSquare className="w-4 h-4 text-gray-600" />
              <h4 className="font-medium text-gray-600">
                {t("completedTasks")}
              </h4>
            </div>
            <Badge className="bg-gray-100 text-gray-600 border-gray-200 text-xs">
              {completedTasks.length} {t("done")}
            </Badge>
          </div>
          <div className="space-y-3">
            {completedTasks.slice(0, 3).map((task) => (
              <TaskCard key={task.id} task={task} />
            ))}
            {completedTasks.length > 3 && (
              <p className="text-xs text-gray-500 text-center py-2">
                +{completedTasks.length - 3} {t("moreCompleted")}
              </p>
            )}
          </div>
        </div>
      )}

      <Separator className="border-gray-200" />

      {/* Add New Task - Clean Form */}
      {!focusMode && !showNewTaskForm ? (
        <Button
          className="w-full text-gray-600 border-gray-200 hover:bg-gray-50"
          variant="outline"
          onClick={() => { setShowNewTaskForm(true); }}
        >
          <Plus className="w-4 h-4 mr-2" />
          {t("addActionItem")}
        </Button>
      ) : (
        !focusMode && (
          <div className="p-4 border border-gray-200 rounded-lg bg-gray-25">
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h5 className="font-medium text-gray-900">
                  {t("newActionItem")}
                </h5>
                <Button
                  className="h-6 w-6 p-0"
                  disabled={isSubmitting}
                  size="sm"
                  variant="ghost"
                  onClick={() => { setShowNewTaskForm(false); }}
                >
                  <X className="w-4 h-4" />
                </Button>
              </div>

              <div className="space-y-3">
                <div>
                  <label
                    className="text-sm font-medium text-gray-700 mb-1 block"
                    htmlFor="task-description"
                  >
                    {t("taskDescription")}:
                  </label>
                  <Textarea
                    className="min-h-[60px] border-gray-300 focus:border-blue-400 focus:ring-blue-100 bg-white"
                    disabled={isSubmitting}
                    id="task-description"
                    placeholder={t("taskDescriptionPlaceholder")}
                    value={newTask.task}
                    onKeyDown={handleKeyDown}
                    onChange={(e) =>
                      { setNewTask({ ...newTask, task: e.target.value }); }
                    }
                  />
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label
                      className="text-sm font-medium text-gray-700 mb-1 block"
                      htmlFor="task-priority"
                    >
                      {t("priority")}:
                    </label>
                    <select
                      className="w-full p-2 text-sm border border-gray-300 rounded-lg bg-white focus:border-blue-400 focus:ring-blue-100"
                      disabled={isSubmitting}
                      id="task-priority"
                      value={newTask.priority}
                      onChange={(e) =>
                        { setNewTask({
                          ...newTask,
                          priority: e.target.value as "low" | "medium" | "high",
                        }); }
                      }
                    >
                      <option value="low">{t("lowPriority")}</option>
                      <option value="medium">{t("mediumPriority")}</option>
                      <option value="high">{t("highPriority")}</option>
                    </select>
                  </div>

                  <div>
                    <label
                      className="text-sm font-medium text-gray-700 mb-1 block"
                      htmlFor="task-due-time"
                    >
                      {t("dueTime")}:
                    </label>
                    <input
                      className="w-full p-2 text-sm border border-gray-300 rounded-lg bg-white focus:border-blue-400 focus:ring-blue-100"
                      disabled={isSubmitting}
                      id="task-due-time"
                      placeholder={t("dueTimePlaceholder")}
                      type="text"
                      value={newTask.dueTime}
                      onChange={(e) =>
                        { setNewTask({ ...newTask, dueTime: e.target.value }); }
                      }
                    />
                  </div>
                </div>
              </div>

              <div className="flex justify-end space-x-2">
                <Button
                  className="text-xs border-gray-300 hover:bg-gray-50"
                  disabled={isSubmitting}
                  size="sm"
                  variant="outline"
                  onClick={() => { setShowNewTaskForm(false); }}
                >
                  {t("cancel")}
                </Button>
                <Button
                  className="text-xs bg-blue-600 hover:bg-blue-700 text-white"
                  disabled={!newTask.task.trim() || isSubmitting}
                  size="sm"
                  onClick={handleSubmitTask}
                >
                  {isSubmitting ? (
                    <>
                      <div className="w-3 h-3 border border-white border-t-transparent rounded-full animate-spin mr-1"></div>
                      {t("submitting")}
                    </>
                  ) : (
                    <>
                      <Send className="w-3 h-3 mr-1" />
                      {t("submitTask")}
                    </>
                  )}
                </Button>
              </div>
            </div>
          </div>
        )
      )}
    </div>
  );
}
