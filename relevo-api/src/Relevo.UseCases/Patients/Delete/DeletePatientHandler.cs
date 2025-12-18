using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Patients.Delete;

public class DeletePatientHandler(IPatientRepository _patientRepository)
  : ICommandHandler<DeletePatientCommand, Result>
{
  public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Check if patient exists
      var patient = await _patientRepository.GetPatientByIdAsync(request.PatientId);
      
      if (patient == null)
      {
        return Result.NotFound($"Patient with ID {request.PatientId} not found");
      }

      // Delete patient
      var success = await _patientRepository.DeletePatientAsync(request.PatientId);

      if (!success)
      {
        return Result.Error("Failed to delete patient");
      }

      return Result.Success();
    }
    catch (Exception ex)
    {
      // Handle any errors (including database errors)
      return Result.Error($"Error deleting patient: {ex.Message}");
    }
  }
}

