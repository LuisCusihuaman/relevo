using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetById;

public record GetPatientByIdQuery(string PatientId) : IQuery<Result<PatientDetailRecord>>;

