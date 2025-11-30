using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.UpdateClinicalData;

public record UpdateHandoverClinicalDataCommand(string HandoverId, string IllnessSeverity, string SummaryText, string UserId) : ICommand<Result>;

