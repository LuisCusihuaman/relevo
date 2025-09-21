using Microsoft.Extensions.DependencyInjection;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.UseCases.Contributors;
using Relevo.UseCases.Setup;

namespace Relevo.UseCases.Setup;

public static class SetupServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IShiftBoundaryResolver, ShiftBoundaryResolver>();
        // Contributor services are registered elsewhere
        // services.AddScoped<IGetContributorByIdUseCase, GetContributorByIdUseCase>();
        // services.AddScoped<IListContributorsUseCase, ListContributorsUseCase>();
        // services.AddScoped<IGetPatientHandoversUseCase, GetPatientHandoversUseCase>();
        // services.AddScoped<IGetHandoverByIdUseCase, GetHandoverByIdUseCase>();
        return services;
    }
}
