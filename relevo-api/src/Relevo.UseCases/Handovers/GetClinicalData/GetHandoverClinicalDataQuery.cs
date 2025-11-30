using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetClinicalData;

public record GetHandoverClinicalDataQuery(string HandoverId) : IQuery<Result<HandoverClinicalDataRecord>>;

