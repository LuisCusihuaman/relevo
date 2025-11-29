using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.Infrastructure.Data;
using Relevo.UseCases.ShiftCheckIn;
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

    // Register new repositories
    services.AddScoped<IUnitRepository, Persistence.Oracle.Repositories.OracleUnitRepository>();
    services.AddScoped<IShiftRepository, Persistence.Oracle.Repositories.OracleShiftRepository>();
    services.AddScoped<IPatientRepository, Persistence.Oracle.Repositories.OraclePatientRepository>();
    services.AddScoped<IAssignmentRepository, Persistence.Oracle.Repositories.OracleAssignmentRepository>();
    services.AddScoped<IHandoverRepository, Persistence.Oracle.Repositories.OracleHandoverRepository>();
    services.AddScoped<IHandoverSectionsRepository, Persistence.Oracle.Repositories.OracleHandoverSectionsRepository>();
    services.AddScoped<IHandoverMessagingRepository, Persistence.Oracle.Repositories.OracleHandoverMessagingRepository>();
    services.AddScoped<IHandoverActivityRepository, Persistence.Oracle.Repositories.OracleHandoverActivityRepository>();
    services.AddScoped<IHandoverChecklistRepository, Persistence.Oracle.Repositories.OracleHandoverChecklistRepository>();
    services.AddScoped<IHandoverContingencyRepository, Persistence.Oracle.Repositories.OracleHandoverContingencyRepository>();
    services.AddScoped<IHandoverActionItemsRepository, Persistence.Oracle.Repositories.OracleHandoverActionItemsRepository>();
    services.AddScoped<IHandoverParticipantsRepository, Persistence.Oracle.Repositories.OracleHandoverParticipantsRepository>();
    services.AddScoped<IHandoverSyncStatusRepository, Persistence.Oracle.Repositories.OracleHandoverSyncStatusRepository>();
    services.AddScoped<IUserRepository, Persistence.Oracle.Repositories.OracleUserRepository>();
    services.AddScoped<IPatientSummaryRepository, Persistence.Oracle.Repositories.OraclePatientSummaryRepository>();

    // Setup Use Cases
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.AssignPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetMyPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetMyHandoversUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetPatientHandoversUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetUnitsUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetShiftsUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetPatientsByUnitUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetAllPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetHandoverByIdUseCase>();
    services.AddScoped<Relevo.UseCases.ShiftCheckIn.GetPatientByIdUseCase>();

    // Setup Application Service
    services.AddScoped<Relevo.Core.Interfaces.IShiftCheckInService, Relevo.UseCases.ShiftCheckIn.ShiftCheckInService>();

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
