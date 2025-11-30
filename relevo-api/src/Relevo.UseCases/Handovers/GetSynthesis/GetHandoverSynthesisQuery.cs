using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetSynthesis;

public record GetHandoverSynthesisQuery(string HandoverId) : IQuery<Result<HandoverSynthesisRecord>>;

