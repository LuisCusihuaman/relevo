using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Setup;

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
      PatientName = handover.PatientName,
      Status = handover.Status,
      IllnessSeverity = new GetHandoverByIdResponse.IllnessSeverityDto
      {
        Value = handover.IllnessSeverity.Value
      },
      PatientSummary = new GetHandoverByIdResponse.PatientSummaryDto
      {
        Value = handover.PatientSummary.Value
      },
      ActionItems = handover.ActionItems.Select(item => new GetHandoverByIdResponse.ActionItemDto
      {
        Id = item.Id,
        Description = item.Description,
        IsCompleted = item.IsCompleted
      }).ToList(),
      SituationAwarenessDocId = handover.SituationAwarenessDocId,
      Synthesis = handover.Synthesis != null ? new GetHandoverByIdResponse.SynthesisDto
      {
        Value = handover.Synthesis.Value
      } : null,
      ShiftName = handover.ShiftName,
      CreatedBy = handover.CreatedBy,
      AssignedTo = handover.AssignedTo,
      CreatedAt = handover.CreatedAt
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetHandoverByIdRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

public class GetHandoverByIdResponse
{
  public string Id { get; set; } = string.Empty;
  public string AssignmentId { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; }
  public string Status { get; set; } = string.Empty;
  public IllnessSeverityDto IllnessSeverity { get; set; } = new();
  public PatientSummaryDto PatientSummary { get; set; } = new();
  public List<ActionItemDto> ActionItems { get; set; } = [];
  public string? SituationAwarenessDocId { get; set; }
  public SynthesisDto? Synthesis { get; set; }
  public string ShiftName { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public string AssignedTo { get; set; } = string.Empty;
  public string? CreatedAt { get; set; }

  public class IllnessSeverityDto
  {
    public string Value { get; set; } = string.Empty;
  }

  public class PatientSummaryDto
  {
    public string Value { get; set; } = string.Empty;
  }

  public class ActionItemDto
  {
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
  }

  public class SynthesisDto
  {
    public string Value { get; set; } = string.Empty;
  }
}
