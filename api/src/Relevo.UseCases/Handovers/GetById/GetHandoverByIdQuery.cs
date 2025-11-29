using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetById;

public record GetHandoverByIdQuery(string HandoverId) : IQuery<Result<HandoverDetailRecord>>;

