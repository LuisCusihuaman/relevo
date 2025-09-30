using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Setup;
using Relevo.Infrastructure.Data.Oracle;
using Dapper;

namespace Relevo.Web.Handovers;

public class GetHandoverByIdEndpoint(ISetupService _setupService)
  : Endpoint<GetHandoverByIdRequest, GetHandoverByIdResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetHandoverByIdRequest req, CancellationToken ct)
  {
    var handover = await _setupService.GetHandoverByIdAsync(req.HandoverId);

    if (handover == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new GetHandoverByIdResponse
    {
      Id = handover.Id,
      AssignmentId = handover.AssignmentId,
      PatientId = handover.PatientId,
      PatientName = handover.PatientName, // For display in lists/cards
      Status = handover.Status,
      illnessSeverity = new GetHandoverByIdResponse.IllnessSeverityDto
      {
        severity = handover.IllnessSeverity.Severity
      },
      patientSummary = new GetHandoverByIdResponse.PatientSummaryDto
      {
        content = handover.PatientSummary.Content
      },
      actionItems = handover.ActionItems.Select(item => new GetHandoverByIdResponse.ActionItemDto
      {
        id = item.Id,
        description = item.Description,
        isCompleted = item.IsCompleted
      }).ToList(),
      situationAwarenessDocId = handover.SituationAwarenessDocId,
      synthesis = handover.Synthesis != null ? new GetHandoverByIdResponse.SynthesisDto
      {
        content = handover.Synthesis.Content
      } : null,
      ShiftName = handover.ShiftName,
      CreatedBy = handover.CreatedBy,
      AssignedTo = handover.AssignedTo,
      ReceiverUserId = handover.ReceiverUserId,
      CreatedAt = handover.CreatedAt,
      ReadyAt = handover.ReadyAt,
      StartedAt = handover.StartedAt,
      AcknowledgedAt = handover.AcknowledgedAt,
      AcceptedAt = handover.AcceptedAt,
      CompletedAt = handover.CompletedAt,
      CancelledAt = handover.CancelledAt,
      RejectedAt = handover.RejectedAt,
      RejectionReason = handover.RejectionReason,
      ExpiredAt = handover.ExpiredAt,
      HandoverType = handover.HandoverType,
      HandoverWindowDate = handover.HandoverWindowDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
      FromShiftId = handover.FromShiftId,
      ToShiftId = handover.ToShiftId,
      ToDoctorId = handover.ToDoctorId,
      StateName = handover.StateName
    };

    await SendAsync(Response, cancellation: ct);
  }
}

/// <summary>
/// Returns all patient-related data for a handover.
/// This endpoint provides patient demographics, physician assignments, and medical status.
/// Consolidates what was previously split between /patient and /patient-data.
/// Physicians get real shift times from USER_ASSIGNMENTS + SHIFTS tables and status based on handover state.
/// </summary>
public class GetPatientHandoverDataEndpoint(ISetupService setupService, IPhysicianShiftService physicianShiftService)
  : Endpoint<GetPatientHandoverDataRequest, GetPatientHandoverDataResponse>
{
  private readonly ISetupService _setupService = setupService;
  private readonly IPhysicianShiftService _physicianShiftService = physicianShiftService;
  public override void Configure()
  {
    Get("/handovers/{handoverId}/patient");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetPatientHandoverDataRequest req, CancellationToken ct)
  {
    var handover = await _setupService.GetHandoverByIdAsync(req.HandoverId);

    if (handover == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    var patientDetails = await _setupService.GetPatientByIdAsync(handover.PatientId);

    if (patientDetails == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    // PHYSICIAN DATA CALCULATION - NOW USING REAL SETUP DATA!
    // ===============================
    // 1. NAMES: From handover.CreatedByName and handover.AssignedToName ✅
    // 2. ROLE: "Doctor" for all users (simplified) ✅
    // 3. COLOR: Empty (TODO: User preferences) ❌
    // 4. SHIFT TIMES: From USER_ASSIGNMENTS + SHIFTS tables ✅
    // 5. STATUS: Calculated from handover state + physician relationship ✅

    // Get physician shift information from existing USER_ASSIGNMENTS + SHIFTS data
    var assignedShiftTimes = await GetPhysicianShiftTimesAsync(handover.CreatedBy);
    var receivingShiftTimes = await GetPhysicianShiftTimesAsync(handover.AssignedTo);

    // Get patient data for lastEditedBy and updatedAt fields
    var patientData = await _setupService.GetPatientDataAsync(req.HandoverId);

    var assignedPhysicianData = handover.CreatedByName != null ? new GetPatientHandoverDataResponse.PhysicianDto
    {
      name = handover.CreatedByName,           // ✅ From handover
      role = "Doctor",                         // ✅ Simplified role
      color = string.Empty,                    // ❌ TODO: User preferences
      shiftEnd = assignedShiftTimes.endTime,  // ✅ From SHIFTS table via USER_ASSIGNMENTS
      shiftStart = assignedShiftTimes.startTime, // ✅ From SHIFTS table via USER_ASSIGNMENTS
      status = CalculatePhysicianStatus(handover, "creator"), // ✅ Based on handover state
      patientAssignment = "assigned"
    } : null;

    var receivingPhysicianData = handover.AssignedToName != null ? new GetPatientHandoverDataResponse.PhysicianDto
    {
      name = handover.AssignedToName,          // ✅ From handover
      role = "Doctor",                         // ✅ Simplified role
      color = string.Empty,                    // ❌ TODO: User preferences
      shiftEnd = receivingShiftTimes.endTime, // ✅ From SHIFTS table via USER_ASSIGNMENTS
      shiftStart = receivingShiftTimes.startTime, // ✅ From SHIFTS table via USER_ASSIGNMENTS
      status = CalculatePhysicianStatus(handover, "assignee"), // ✅ Based on handover state
      patientAssignment = "receiving"
    } : null;

    // Return ALL patient-related data for this handover (consolidated from /patient and /patient-data)
    Response = new GetPatientHandoverDataResponse
    {
      id = handover.PatientId,
      name = patientDetails.Name,
      dob = patientDetails.Dob, // Raw DOB for age calculation
      mrn = patientDetails.Mrn,
      admissionDate = patientDetails.AdmissionDate,
      currentDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
      primaryTeam = patientDetails.CurrentUnit,
      primaryDiagnosis = patientDetails.Diagnosis,
      room = patientDetails.RoomNumber,
      unit = patientDetails.CurrentUnit,
      assignedPhysician = assignedPhysicianData,
      receivingPhysician = receivingPhysicianData,
      // Medical status data (previously in /patient-data endpoint)
      illnessSeverity = handover.IllnessSeverity?.Severity,
      summaryText = handover.PatientSummary?.Content,
      lastEditedBy = patientData?.LastEditedBy,
      updatedAt = patientData?.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
    };

    await SendAsync(Response, cancellation: ct);
  }

  private async Task<(string startTime, string endTime)> GetPhysicianShiftTimesAsync(string? userId)
  {
    // Now using proper hexagonal architecture - domain service handles the business logic
    return await _physicianShiftService.GetPhysicianShiftTimesAsync(userId ?? string.Empty);
  }

  private static string CalculatePhysicianStatus(HandoverRecord handover, string relationship)
  {
    // Calculate physician status based on handover state and their relationship
    var state = handover.StateName?.ToLower();

    // Most states map the same way regardless of relationship
    return state switch
    {
      "completed" => "completed",
      "cancelled" => "cancelled",
      "rejected" => "rejected",
      "expired" => "expired",
      "accepted" => relationship == "creator" ? "handed-off" : "accepted",
      "draft" => relationship == "creator" ? "handing-off" : "pending",
      "ready" => relationship == "creator" ? "handing-off" : "ready-to-receive",
      "inprogress" => relationship == "creator" ? "handing-off" : "receiving",
      _ => "unknown"
    };
  }
}

public class GetHandoverByIdRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

public class GetPatientHandoverDataRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

public class GetHandoverByIdResponse
{
  public string Id { get; set; } = string.Empty;
  public string AssignmentId { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; } // For display in lists/cards
  public string Status { get; set; } = string.Empty;
  public IllnessSeverityDto illnessSeverity { get; set; } = new();
  public PatientSummaryDto patientSummary { get; set; } = new();
  public List<ActionItemDto> actionItems { get; set; } = [];
  public string? situationAwarenessDocId { get; set; }
  public SynthesisDto? synthesis { get; set; }
  public string ShiftName { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public string AssignedTo { get; set; } = string.Empty;
  public string? ReceiverUserId { get; set; }
  public string? CreatedAt { get; set; }
  public string? ReadyAt { get; set; }
  public string? StartedAt { get; set; }
  public string? AcknowledgedAt { get; set; }
  public string? AcceptedAt { get; set; }
  public string? CompletedAt { get; set; }
  public string? CancelledAt { get; set; }
  public string? RejectedAt { get; set; }
  public string? RejectionReason { get; set; }
  public string? ExpiredAt { get; set; }
  public string? HandoverType { get; set; }
  public string? HandoverWindowDate { get; set; }
  public string? FromShiftId { get; set; }
  public string? ToShiftId { get; set; }
  public string? ToDoctorId { get; set; }
  public string StateName { get; set; } = string.Empty;

  public class IllnessSeverityDto
  {
    public string severity { get; set; } = string.Empty;
  }

  public class PatientSummaryDto
  {
    public string content { get; set; } = string.Empty;
  }

  public class ActionItemDto
  {
    public string id { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool isCompleted { get; set; }
  }

  public class SynthesisDto
  {
    public string content { get; set; } = string.Empty;
  }
}

public class GetPatientHandoverDataResponse
{
  public string id { get; set; } = string.Empty;
  public string name { get; set; } = string.Empty;
  public string dob { get; set; } = string.Empty; // Return raw DOB, client handles age calculation
  public string mrn { get; set; } = string.Empty;
  public string admissionDate { get; set; } = string.Empty;
  public string currentDateTime { get; set; } = string.Empty;
  public string primaryTeam { get; set; } = string.Empty;
  public string primaryDiagnosis { get; set; } = string.Empty;
  public string room { get; set; } = string.Empty;
  public string unit { get; set; } = string.Empty;
  public PhysicianDto? assignedPhysician { get; set; }
  public PhysicianDto? receivingPhysician { get; set; }
  // Medical status data (consolidated from /patient-data endpoint)
  public string? illnessSeverity { get; set; }
  public string? summaryText { get; set; }
  public string? lastEditedBy { get; set; }
  public string? updatedAt { get; set; }

  public class PhysicianDto
  {
    public string name { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty; // Should come from user data, not hardcoded
    public string color { get; set; } = string.Empty; // Should come from user preferences
    public string? shiftEnd { get; set; } // Should come from shift configuration
    public string? shiftStart { get; set; } // Should come from shift configuration
    public string status { get; set; } = string.Empty; // Should come from handover state
    public string patientAssignment { get; set; } = string.Empty;
  }

  // Note: This endpoint provides PATIENT INFO (demographics + physicians)
  // The /handovers/{id}/patient-data endpoint provides PATIENT DATA (medical info within handover)
}
