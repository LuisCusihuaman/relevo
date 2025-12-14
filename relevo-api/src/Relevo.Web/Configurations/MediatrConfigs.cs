using Ardalis.SharedKernel;
using Relevo.Core.Handlers;
using Relevo.UseCases.Handovers.StateMachine;
using MediatR;
using System.Reflection;

namespace Relevo.Web.Configurations;

public static class MediatrConfigs
{
  public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
  {
    var mediatRAssemblies = new[]
      {
        Assembly.GetAssembly(typeof(PatientAssignedToShiftHandler)), // Core
        Assembly.GetAssembly(typeof(HandoverStateMachineHandlers)) // UseCases
      };

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

    return services;
  }
}
