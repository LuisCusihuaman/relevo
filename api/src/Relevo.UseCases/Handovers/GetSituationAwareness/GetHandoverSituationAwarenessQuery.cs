using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetSituationAwareness;

public record GetHandoverSituationAwarenessQuery(string HandoverId) : IQuery<Result<HandoverSituationAwarenessRecord>>;

