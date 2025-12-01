using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.GetPending;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class GetPendingHandovers(IMediator _mediator, ICurrentUser _currentUser)
  : EndpointWithoutRequest<GetPendingHandoversResponse>
{
  public override void Configure()
  {
    Get("/handovers/pending");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }
    
    var result = await _mediator.Send(new GetPendingHandoversQuery(userId), ct);

    if (result.IsSuccess)
    {
        Response = new GetPendingHandoversResponse
        {
            Handovers = result.Value.Select(h => new HandoverDto
            {
                Id = h.Id,
                PatientId = h.PatientId,
                PatientName = h.PatientName,
                Status = h.Status,
                ShiftName = h.ShiftName ?? ""
            }).ToList()
        };
        await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetPendingHandoversResponse
{
    public List<HandoverDto> Handovers { get; set; } = new();
}

public class HandoverDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
}

