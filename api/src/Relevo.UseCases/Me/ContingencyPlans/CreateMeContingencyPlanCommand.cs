using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ContingencyPlans;

public record CreateMeContingencyPlanCommand(
    string HandoverId,
    string ConditionText,
    string ActionText,
    string Priority,
    string UserId
) : IRequest<Result<ContingencyPlanRecord>>;

