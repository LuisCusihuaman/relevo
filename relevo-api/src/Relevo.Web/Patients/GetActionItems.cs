using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.GetActionItems;
using Relevo.Core.Models;

namespace Relevo.Web.Patients;

public class GetPatientActionItems(IMediator _mediator)
  : Endpoint<GetPatientActionItemsRequest, GetPatientActionItemsResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}/action-items");
  }

  public override async Task HandleAsync(GetPatientActionItemsRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetPatientActionItemsQuery(req.PatientId), ct);

    if (result.IsSuccess)
    {
        Response = new GetPatientActionItemsResponse
        {
            ActionItems = result.Value.Select(ai => new PatientActionItemDto
            {
                Id = ai.Id,
                HandoverId = ai.HandoverId,
                Description = ai.Description,
                IsCompleted = ai.IsCompleted,
                CreatedAt = ai.CreatedAt,
                CreatedBy = ai.CreatedBy,
                ShiftName = ai.ShiftName
            }).ToList()
        };
        await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetPatientActionItemsRequest
{
    public string PatientId { get; set; } = string.Empty;
}

public class GetPatientActionItemsResponse
{
    public List<PatientActionItemDto> ActionItems { get; set; } = [];
}

public class PatientActionItemDto
{
    public string Id { get; set; } = string.Empty;
    public string HandoverId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
}

