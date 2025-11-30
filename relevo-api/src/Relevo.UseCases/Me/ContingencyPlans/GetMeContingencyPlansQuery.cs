using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ContingencyPlans;

public record GetMeContingencyPlansQuery(string HandoverId) : IRequest<Result<IReadOnlyList<ContingencyPlanRecord>>>;

