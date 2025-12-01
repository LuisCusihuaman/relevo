using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.Infrastructure.Data;
using Relevo.Infrastructure.Data.Queries;
using Relevo.UseCases.Contributors.List;
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
    // Register SmartEnum Type Handler
    SqlMapper.AddTypeHandler(typeof(ContributorStatus), new SmartEnumByValueTypeHandler<ContributorStatus>());

    // Configure Dapper Global Settings
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    services.AddSingleton<DapperConnectionFactory>();

    // Register Dapper Repositories (Specific)
    services.AddScoped<IContributorRepository, ContributorRepository>();
    services.AddScoped<IPatientRepository, PatientRepository>();
    services.AddScoped<IShiftRepository, ShiftRepository>();
    services.AddScoped<IShiftInstanceRepository, ShiftInstanceRepository>();
    services.AddScoped<IShiftWindowRepository, ShiftWindowRepository>();
    services.AddScoped<IUnitRepository, UnitRepository>();
    services.AddScoped<IHandoverRepository, HandoverRepository>();
    services.AddScoped<IAssignmentRepository, AssignmentRepository>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IShiftTransitionService, ShiftTransitionService>();

    services.AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
           .AddScoped<IDeleteContributorService, DeleteContributorService>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
