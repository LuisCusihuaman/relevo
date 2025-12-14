using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.GetById;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Patients;

public class GetPatientById(IMediator _mediator)
  : Endpoint<GetPatientByIdRequest, GetPatientByIdResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}");
  }

  public override async Task HandleAsync(GetPatientByIdRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetPatientByIdQuery(req.PatientId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      var patient = result.Value;
      Response = new GetPatientByIdResponse
      {
        Id = patient.Id,
        Name = patient.Name,
        Mrn = patient.Mrn,
        Dob = patient.Dob,
        Gender = patient.Gender,
        AdmissionDate = patient.AdmissionDate,
        CurrentUnit = patient.CurrentUnit,
        RoomNumber = patient.RoomNumber,
        Diagnosis = patient.Diagnosis,
        Allergies = patient.Allergies.ToList(),
        Medications = patient.Medications.ToList(),
        Notes = patient.Notes,
        Weight = patient.Weight,
        Height = patient.Height
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetPatientByIdRequest
{
  public string PatientId { get; set; } = string.Empty;
}

public class GetPatientByIdResponse
{
  [Required]
  public required string Id { get; set; }
  [Required]
  public required string Name { get; set; }
  [Required]
  public required string Mrn { get; set; }
  [Required]
  public required string Dob { get; set; }
  [Required]
  public required string Gender { get; set; }
  [Required]
  public required string AdmissionDate { get; set; }
  [Required]
  public required string CurrentUnit { get; set; }
  [Required]
  public required string RoomNumber { get; set; }
  [Required]
  public required string Diagnosis { get; set; }
  [Required]
  public required List<string> Allergies { get; set; }
  [Required]
  public required List<string> Medications { get; set; }
  [Required]
  public required string Notes { get; set; }
  public string? Weight { get; set; }
  public string? Height { get; set; }
}

