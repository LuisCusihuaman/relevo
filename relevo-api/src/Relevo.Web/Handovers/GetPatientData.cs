using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetPatientData;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Handovers;

public class GetPatientHandoverData(IMediator _mediator)
  : Endpoint<GetPatientHandoverDataRequest, GetPatientHandoverDataResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/patient");
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
  [Required]
  public required string id { get; set; }
  [Required]
  public required string name { get; set; }
  [Required]
  public required string dob { get; set; }
  [Required]
  public required string mrn { get; set; }
  [Required]
  public required string admissionDate { get; set; }
  [Required]
  public required string currentDateTime { get; set; }
  [Required]
  public required string primaryTeam { get; set; }
  [Required]
  public required string primaryDiagnosis { get; set; }
  [Required]
  public required string room { get; set; }
  [Required]
  public required string unit { get; set; }
  public PhysicianDto? assignedPhysician { get; set; }
  public PhysicianDto? receivingPhysician { get; set; }
  public string? illnessSeverity { get; set; }
  public string? summaryText { get; set; }
  public string? lastEditedBy { get; set; }
  public string? updatedAt { get; set; }

  public class PhysicianDto
  {
    [Required]
    public required string name { get; set; }
    [Required]
    public required string role { get; set; }
    [Required]
    public required string color { get; set; }
    public string? shiftEnd { get; set; }
    public string? shiftStart { get; set; }
    [Required]
    public required string status { get; set; }
    [Required]
    public required string patientAssignment { get; set; }
  }
}

