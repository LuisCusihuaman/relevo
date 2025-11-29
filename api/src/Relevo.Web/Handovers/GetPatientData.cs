using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetPatientData;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class GetPatientHandoverData(IMediator _mediator)
  : Endpoint<GetPatientHandoverDataRequest, GetPatientHandoverDataResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/patient");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetPatientHandoverDataRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetPatientHandoverDataQuery(req.HandoverId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      var data = result.Value;
      Response = new GetPatientHandoverDataResponse
      {
        id = data.Id,
        name = data.Name,
        dob = data.Dob,
        mrn = data.Mrn,
        admissionDate = data.AdmissionDate,
        currentDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
        primaryTeam = data.PrimaryTeam,
        primaryDiagnosis = data.PrimaryDiagnosis,
        room = data.Room,
        unit = data.Unit,
        assignedPhysician = data.AssignedPhysician != null ? new GetPatientHandoverDataResponse.PhysicianDto
        {
          name = data.AssignedPhysician.Name,
          role = data.AssignedPhysician.Role,
          color = data.AssignedPhysician.Color,
          shiftStart = data.AssignedPhysician.ShiftStart,
          shiftEnd = data.AssignedPhysician.ShiftEnd,
          status = data.AssignedPhysician.Status,
          patientAssignment = data.AssignedPhysician.PatientAssignment
        } : null,
        receivingPhysician = data.ReceivingPhysician != null ? new GetPatientHandoverDataResponse.PhysicianDto
        {
          name = data.ReceivingPhysician.Name,
          role = data.ReceivingPhysician.Role,
          color = data.ReceivingPhysician.Color,
          shiftStart = data.ReceivingPhysician.ShiftStart,
          shiftEnd = data.ReceivingPhysician.ShiftEnd,
          status = data.ReceivingPhysician.Status,
          patientAssignment = data.ReceivingPhysician.PatientAssignment
        } : null,
        illnessSeverity = data.IllnessSeverity,
        summaryText = data.SummaryText,
        lastEditedBy = data.LastEditedBy,
        updatedAt = data.UpdatedAt
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetPatientHandoverDataRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

public class GetPatientHandoverDataResponse
{
  public string id { get; set; } = string.Empty;
  public string name { get; set; } = string.Empty;
  public string dob { get; set; } = string.Empty;
  public string mrn { get; set; } = string.Empty;
  public string admissionDate { get; set; } = string.Empty;
  public string currentDateTime { get; set; } = string.Empty;
  public string primaryTeam { get; set; } = string.Empty;
  public string primaryDiagnosis { get; set; } = string.Empty;
  public string room { get; set; } = string.Empty;
  public string unit { get; set; } = string.Empty;
  public PhysicianDto? assignedPhysician { get; set; }
  public PhysicianDto? receivingPhysician { get; set; }
  public string? illnessSeverity { get; set; }
  public string? summaryText { get; set; }
  public string? lastEditedBy { get; set; }
  public string? updatedAt { get; set; }

  public class PhysicianDto
  {
    public string name { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public string color { get; set; } = string.Empty;
    public string? shiftEnd { get; set; }
    public string? shiftStart { get; set; }
    public string status { get; set; } = string.Empty;
    public string patientAssignment { get; set; } = string.Empty;
  }
}

