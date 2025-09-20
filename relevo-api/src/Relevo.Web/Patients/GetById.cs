using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Setup;

namespace Relevo.Web.Patients;

public class GetPatientByIdEndpoint : Endpoint<GetPatientByIdRequest, GetPatientByIdResponse>
{
    private readonly ISetupService _setupService;

    public GetPatientByIdEndpoint(ISetupService setupService)
    {
        _setupService = setupService;
    }

    public override void Configure()
  {
    Get("/patients/{patientId}");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetPatientByIdRequest req, CancellationToken ct)
  {
    var patient = await _setupService.GetPatientByIdAsync(req.PatientId);

    if (patient == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

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
      Notes = patient.Notes
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetPatientByIdRequest
{
  public string PatientId { get; set; } = string.Empty;
}

public class GetPatientByIdResponse
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Mrn { get; set; } = string.Empty;
  public string Dob { get; set; } = string.Empty;
  public string Gender { get; set; } = string.Empty;
  public string AdmissionDate { get; set; } = string.Empty;
  public string CurrentUnit { get; set; } = string.Empty;
  public string RoomNumber { get; set; } = string.Empty;
  public string Diagnosis { get; set; } = string.Empty;
  public List<string> Allergies { get; set; } = [];
  public List<string> Medications { get; set; } = [];
  public string Notes { get; set; } = string.Empty;
}
