import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Clock, UserPlus, CheckCircle } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useCreateHandover, usePendingHandovers, useAcceptHandover, useCompleteHandover } from "@/api";
import { useState, type JSX } from "react";

interface ShiftTransitionProps {
  currentUser: {
    id: string;
    name: string;
    initials: string;
    role: string;
  };
  patients: Array<{
    id: string;
    name: string;
    room: string;
    diagnosis: string;
  }>;
  availableDoctors: Array<{
    id: string;
    name: string;
    initials: string;
    role: string;
    shift: string;
  }>;
}

export function ShiftTransition({ currentUser, patients, availableDoctors }: ShiftTransitionProps): JSX.Element {
  const { t } = useTranslation("shiftTransition");
  const [selectedPatient, setSelectedPatient] = useState<string | null>(null);
  const [selectedDoctor, setSelectedDoctor] = useState<string | null>(null);

  const createHandoverMutation = useCreateHandover();
  const { data: pendingHandovers } = usePendingHandovers(currentUser.id);
  const acceptHandoverMutation = useAcceptHandover();
  const completeHandoverMutation = useCompleteHandover();

  const handleInitiateHandover = (): void => {
    if (!selectedPatient || !selectedDoctor) return;

    const targetDoctor = availableDoctors.find(d => d.id === selectedDoctor);
    if (!targetDoctor) return;

    createHandoverMutation.mutate({
      patientId: selectedPatient,
      fromDoctorId: currentUser.id,
      toDoctorId: selectedDoctor,
      fromShiftId: currentUser.role.includes("Day") ? "shift-day" : "shift-night",
      toShiftId: targetDoctor.shift === "Day" ? "shift-day" : "shift-night",
      initiatedBy: currentUser.id,
      notes: `Handover initiated by ${currentUser.name} for patient transition`,
    });
  };

  const handleAcceptHandover = (handoverId: string): void => {
    acceptHandoverMutation.mutate(handoverId);
  };

  const handleCompleteHandover = (handoverId: string): void => {
    completeHandoverMutation.mutate(handoverId);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-900 mb-2">
          {t("title")}
        </h2>
        <p className="text-gray-600">
          {t("subtitle", { name: currentUser.name })}
        </p>
      </div>

      {/* Current Role Status */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center space-x-2">
            <UserPlus className="w-5 h-5" />
            <span>{t("currentRole")}</span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center space-x-3">
            <Avatar>
              <AvatarFallback>{currentUser.initials}</AvatarFallback>
            </Avatar>
            <div>
              <p className="font-medium">{currentUser.name}</p>
              <p className="text-sm text-gray-600">{currentUser.role}</p>
            </div>
            <Badge variant="secondary">
              <Clock className="w-3 h-3 mr-1" />
              {t("active")}
            </Badge>
          </div>
        </CardContent>
      </Card>

      {/* Initiate Handover Section */}
      <Card>
        <CardHeader>
          <CardTitle>{t("initiateHandover.title")}</CardTitle>
          <CardDescription>{t("initiateHandover.description")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Patient Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t("selectPatient")}
            </label>
            <div className="grid gap-2">
              {patients.map((patient) => (
                <button
                  key={patient.id}
                  className={`p-3 border rounded-lg text-left transition-colors ${
                    selectedPatient === patient.id
                      ? "border-blue-500 bg-blue-50"
                      : "border-gray-200 hover:border-gray-300"
                  }`}
                  onClick={() => { setSelectedPatient(patient.id); }}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">{patient.name}</p>
                      <p className="text-sm text-gray-600">
                        {t("room")}: {patient.room} • {patient.diagnosis}
                      </p>
                    </div>
                    {selectedPatient === patient.id && (
                      <CheckCircle className="w-5 h-5 text-blue-500" />
                    )}
                  </div>
                </button>
              ))}
            </div>
          </div>

          {/* Doctor Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t("selectReceivingDoctor")}
            </label>
            <div className="grid gap-2">
              {availableDoctors
                .filter(doctor => doctor.id !== currentUser.id)
                .map((doctor) => (
                <button
                  key={doctor.id}
                  className={`p-3 border rounded-lg text-left transition-colors ${
                    selectedDoctor === doctor.id
                      ? "border-blue-500 bg-blue-50"
                      : "border-gray-200 hover:border-gray-300"
                  }`}
                  onClick={() => { setSelectedDoctor(doctor.id); }}
                >
                  <div className="flex items-center space-x-3">
                    <Avatar className="w-8 h-8">
                      <AvatarFallback>{doctor.initials}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1">
                      <p className="font-medium">{doctor.name}</p>
                      <p className="text-sm text-gray-600">{doctor.role}</p>
                    </div>
                    {selectedDoctor === doctor.id && (
                      <CheckCircle className="w-5 h-5 text-blue-500" />
                    )}
                  </div>
                </button>
              ))}
            </div>
          </div>

          {/* Initiate Button */}
          <Button
            className="w-full"
            disabled={!selectedPatient || !selectedDoctor || createHandoverMutation.isPending}
            onClick={handleInitiateHandover}
          >
            {createHandoverMutation.isPending ? t("creating") : t("initiateHandover.button")}
          </Button>
        </CardContent>
      </Card>

      {/* Pending Handovers Section */}
      {pendingHandovers && pendingHandovers.handovers.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>{t("pendingHandovers.title")}</CardTitle>
            <CardDescription>{t("pendingHandovers.description")}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {pendingHandovers.handovers.map((handover) => (
              <div key={handover.id} className="border rounded-lg p-4">
                <div className="flex items-center justify-between mb-3">
                  <div>
                    <h4 className="font-medium">{`Patient ${handover.patientId}`}</h4>
                    <p className="text-sm text-gray-600">{handover.shiftName}</p>
                  </div>
                  <Badge variant={handover.stateName === "Ready" ? "secondary" : "default"}>
                    {handover.stateName}
                  </Badge>
                </div>
                <div className="flex space-x-2">
                  {handover.stateName === "Ready" && (
                    <Button
                      disabled={acceptHandoverMutation.isPending}
                      size="sm"
                      onClick={() => { handleAcceptHandover(handover.id); }}
                    >
                      {t("accept")}
                    </Button>
                  )}
                  {(handover.stateName === "Accepted" || handover.stateName === "InProgress") && (
                    <Button
                      disabled={completeHandoverMutation.isPending}
                      size="sm"
                      onClick={() => { handleCompleteHandover(handover.id); }}
                    >
                      {t("complete")}
                    </Button>
                  )}
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
