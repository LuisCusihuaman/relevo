using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.Create;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class CreateHandover(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<CreateHandoverRequestDto, CreateHandoverResponse>
{
  public override void Configure()
  {
    Post("/handovers");
  }

  public override async Task HandleAsync(CreateHandoverRequestDto req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }
    
    var command = new CreateHandoverCommand(
        req.PatientId,
        req.FromDoctorId,
        req.ToDoctorId,
        req.FromShiftId,
        req.ToShiftId,
        req.InitiatedBy,
        req.Notes
    );

    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      var handover = result.Value;
      Response = new CreateHandoverResponse
      {
        Id = handover.Id,
        PatientId = handover.PatientId,
        PatientName = handover.PatientName,
        Status = handover.Status,
        FromDoctorId = req.FromDoctorId,
        ToDoctorId = req.ToDoctorId,
        FromShiftId = req.FromShiftId,
        ToShiftId = req.ToShiftId,
        CreatedAt = handover.CreatedAt
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class CreateHandoverRequestDto
{
  public string PatientId { get; set; } = string.Empty;
  public string FromDoctorId { get; set; } = string.Empty;
  public string ToDoctorId { get; set; } = string.Empty;
  public string FromShiftId { get; set; } = string.Empty;
  public string ToShiftId { get; set; } = string.Empty;
  public string InitiatedBy { get; set; } = string.Empty;
  public string? Notes { get; set; }
}

public class CreateHandoverResponse
{
  public string Id { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; }
  public string Status { get; set; } = string.Empty;
  public string FromDoctorId { get; set; } = string.Empty;
  public string ToDoctorId { get; set; } = string.Empty;
  public string FromShiftId { get; set; } = string.Empty;
  public string ToShiftId { get; set; } = string.Empty;
  public string? CreatedAt { get; set; }
}

