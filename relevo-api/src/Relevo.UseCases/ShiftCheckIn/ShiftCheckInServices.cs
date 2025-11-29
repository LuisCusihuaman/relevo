using Microsoft.Extensions.DependencyInjection;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.UseCases.ShiftCheckIn;

namespace Relevo.UseCases.ShiftCheckIn;

public static class ShiftCheckInServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IShiftCheckInService, ShiftCheckInService>();
        services.AddScoped<IShiftBoundaryResolver, ShiftBoundaryResolver>();
        services.AddScoped<IHandoverStateService, HandoverStateService>();
        // Contributor services are registered elsewhere
        // services.AddScoped<IGetContributorByIdUseCase, GetContributorByIdUseCase>();
        // services.AddScoped<IListContributorsUseCase, ListContributorsUseCase>();
        // services.AddScoped<IGetPatientHandoversUseCase, GetPatientHandoversUseCase>();
        // services.AddScoped<IGetHandoverByIdUseCase, GetHandoverByIdUseCase>();
        return services;
    }
}
