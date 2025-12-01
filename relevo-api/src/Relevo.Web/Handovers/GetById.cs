using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetById;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class GetHandoverById(IMediator _mediator)
  : Endpoint<GetHandoverByIdRequest, GetHandoverByIdResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}");
  }

  public override async Task HandleAsync(GetHandoverByIdRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetHandoverByIdQuery(req.HandoverId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      var detail = result.Value;
      var handover = detail.Handover;
      
      Response = new GetHandoverByIdResponse
      {
        Id = handover.Id,
        PatientId = handover.PatientId,
        PatientName = handover.PatientName ?? "",
        Status = handover.Status,
        ResponsiblePhysicianId = handover.ResponsiblePhysicianId ?? "",
        ResponsiblePhysicianName = handover.ResponsiblePhysicianName ?? "",
        illnessSeverity = new GetHandoverByIdResponse.IllnessSeverityDto
        {
          severity = handover.IllnessSeverity ?? "Stable"
        },
        patientSummary = new GetHandoverByIdResponse.PatientSummaryDto
        {
          content = handover.PatientSummary ?? ""
        },
        situationAwarenessDocId = handover.SituationAwarenessDocId,
        synthesis = handover.Synthesis != null ? new GetHandoverByIdResponse.SynthesisDto
        {
          content = handover.Synthesis
        } : null,
        actionItems = detail.ActionItems.Select(a => new GetHandoverByIdResponse.ActionItemDto
        {
          id = a.Id,
          description = a.Description,
          isCompleted = a.IsCompleted
        }).ToList(),
        ShiftName = handover.ShiftName ?? "",
        CreatedBy = handover.CreatedBy ?? "",
        AssignedTo = handover.AssignedTo ?? "",
        ReceiverUserId = handover.ReceiverUserId ?? "",
        CreatedAt = handover.CreatedAt,
        ReadyAt = handover.ReadyAt,
        StartedAt = handover.StartedAt,
        CompletedAt = handover.CompletedAt,
        CancelledAt = handover.CancelledAt,
        HandoverWindowDate = handover.HandoverWindowDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
        StateName = handover.StateName,
        Version = handover.Version,
        ShiftWindowId = handover.ShiftWindowId,
        PreviousHandoverId = handover.PreviousHandoverId,
        SenderUserId = handover.SenderUserId,
        ReadyByUserId = handover.ReadyByUserId,
        StartedByUserId = handover.StartedByUserId,
        CompletedByUserId = handover.CompletedByUserId,
        CancelledByUserId = handover.CancelledByUserId,
        CancelReason = handover.CancelReason
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetHandoverByIdRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

public class GetHandoverByIdResponse
{
  public string Id { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; }
  public string Status { get; set; } = string.Empty;
  public string ResponsiblePhysicianId { get; set; } = string.Empty;
  public string ResponsiblePhysicianName { get; set; } = string.Empty;
  public IllnessSeverityDto illnessSeverity { get; set; } = new();
  public PatientSummaryDto patientSummary { get; set; } = new();
  public string? situationAwarenessDocId { get; set; }
  public SynthesisDto? synthesis { get; set; }
  public string ShiftName { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public string AssignedTo { get; set; } = string.Empty;
  public string? ReceiverUserId { get; set; }
  public string? CreatedAt { get; set; }
  public string? ReadyAt { get; set; }
  public string? StartedAt { get; set; }
  public string? CompletedAt { get; set; }
  public string? CancelledAt { get; set; }
  public string? HandoverWindowDate { get; set; }
  public string StateName { get; set; } = string.Empty;
  public int Version { get; set; }
  public List<ActionItemDto> actionItems { get; set; } = new();
  // V3 Fields
  public string? ShiftWindowId { get; set; }
  public string? PreviousHandoverId { get; set; }
  public string? SenderUserId { get; set; }
  public string? ReadyByUserId { get; set; }
  public string? StartedByUserId { get; set; }
  public string? CompletedByUserId { get; set; }
  public string? CancelledByUserId { get; set; }
  public string? CancelReason { get; set; }

  public class IllnessSeverityDto
  {
    public string severity { get; set; } = string.Empty;
  }

  public class PatientSummaryDto
  {
    public string content { get; set; } = string.Empty;
  }

  public class SynthesisDto
  {
    public string content { get; set; } = string.Empty;
  }

  public class ActionItemDto
  {
    public string id { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool isCompleted { get; set; }
  }
}

