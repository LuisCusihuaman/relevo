using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Relevo.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    // Configure Dapper Global Settings
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    services.AddSingleton<DapperConnectionFactory>();

    // Register Dapper Repositories
    services.AddScoped<IPatientRepository, PatientRepository>();
    services.AddScoped<IShiftRepository, ShiftRepository>();
    services.AddScoped<IShiftInstanceRepository, ShiftInstanceRepository>();
    services.AddScoped<IShiftWindowRepository, ShiftWindowRepository>();
    services.AddScoped<IUnitRepository, UnitRepository>();
    services.AddScoped<IHandoverRepository, HandoverRepository>();
    services.AddScoped<IAssignmentRepository, AssignmentRepository>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IShiftTransitionService, ShiftTransitionService>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
