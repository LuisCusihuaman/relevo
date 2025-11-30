using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetPatientData;

public record GetPatientHandoverDataQuery(string HandoverId) : IQuery<Result<PatientHandoverDataRecord>>;

