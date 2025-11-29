using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.DeleteContingencyPlan;

public record DeleteContingencyPlanCommand(string HandoverId, string ContingencyId) : ICommand<Result>;

