using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.Infrastructure.Data;
using Relevo.Infrastructure.Data.Queries;
using Relevo.UseCases.Contributors.List;
using Relevo.UseCases.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Relevo.Infrastructure;
public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration config,
    ILogger logger)
  {
    // Always use Oracle - no more SQLite support
    logger.LogInformation("Infrastructure: Always using Oracle database");

    // Configure Oracle connection factory
    services.AddSingleton<Data.Oracle.IOracleConnectionFactory, Data.Oracle.OracleConnectionFactory>();
    services.AddSingleton<IDbConnectionFactory>(sp => (IDbConnectionFactory)sp.GetRequiredService<Data.Oracle.IOracleConnectionFactory>());

    logger.LogInformation("Oracle database configured - using Oracle repositories");

    // Always use Oracle implementations
    services.AddScoped<Relevo.Core.Interfaces.IContributorService, OracleContributorService>();
    services.AddScoped<IListContributorsQueryService, Data.Queries.OracleListContributorsQueryService>();
    services.AddScoped<IDeleteContributorService, DeleteContributorService>();

    // Always use Oracle setup repository
    services.AddScoped<ISetupRepository, Repositories.OracleSetupRepository>();

    // Setup Use Cases
    services.AddScoped<Relevo.UseCases.Setup.AssignPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetMyPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetMyHandoversUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetPatientHandoversUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetUnitsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetShiftsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetPatientsByUnitUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetAllPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetHandoverByIdUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetPatientByIdUseCase>();

    // Setup Application Service
    services.AddScoped<Relevo.Core.Interfaces.ISetupService, Relevo.UseCases.Setup.SetupService>();

    // Physician shift services following hexagonal architecture
    services.AddScoped<IPhysicianShiftRepository, Repositories.OraclePhysicianShiftRepository>();
    services.AddScoped<IPhysicianShiftService, PhysicianShiftService>();

    // Add core services including ShiftBoundaryResolver
    services.AddCoreServices();

    // Authentication and Authorization Services
    services.AddScoped<Relevo.Core.Interfaces.IAuthenticationService, Auth.ClerkAuthenticationService>();
    services.AddScoped<Relevo.Core.Interfaces.IAuthorizationService, Auth.ClerkAuthorizationService>();
    services.AddScoped<Relevo.Core.Interfaces.IUserContext, Auth.HttpContextUserContext>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
