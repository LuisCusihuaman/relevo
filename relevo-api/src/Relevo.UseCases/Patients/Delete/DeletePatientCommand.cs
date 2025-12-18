using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Patients.Delete;

public record DeletePatientCommand(string PatientId) : ICommand<Result>;

