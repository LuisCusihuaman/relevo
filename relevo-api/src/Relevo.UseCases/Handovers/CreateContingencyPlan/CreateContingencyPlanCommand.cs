using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.CreateContingencyPlan;

public record CreateContingencyPlanCommand(
    string HandoverId,
    string Condition,
    string Action,
    string Priority,
    string UserId
) : ICommand<Result<ContingencyPlanRecord>>;

