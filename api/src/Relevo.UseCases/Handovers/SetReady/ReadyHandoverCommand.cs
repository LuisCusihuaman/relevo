using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.SetReady;

public record ReadyHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;

