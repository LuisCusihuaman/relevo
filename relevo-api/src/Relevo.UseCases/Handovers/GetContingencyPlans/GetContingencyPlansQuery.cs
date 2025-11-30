using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetContingencyPlans;

public record GetContingencyPlansQuery(string HandoverId) : IQuery<Result<IReadOnlyList<ContingencyPlanRecord>>>;

