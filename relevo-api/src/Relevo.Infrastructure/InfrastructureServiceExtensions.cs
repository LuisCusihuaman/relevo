using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.Infrastructure.Data;
using Relevo.Infrastructure.Data.Queries;
using Relevo.Infrastructure.Data.Sqlite;
using Relevo.UseCases.Contributors.List;
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
            // Force SQLite for testing environment by checking environment variables
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            // Debug specific environment variables
            logger.LogInformation("Infrastructure: ASPNETCORE_ENVIRONMENT = {AspNetCore}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            logger.LogInformation("Infrastructure: DOTNET_ENVIRONMENT = {DotNet}", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
            logger.LogInformation("Infrastructure: Environment = {Environment}", environmentName);

            bool useOracle = environmentName == "Testing" ? false : config.GetValue("UseOracle", false);
            bool useOracleForSetup = environmentName == "Testing" ? false : config.GetValue("UseOracleForSetup", false);

            logger.LogInformation("Infrastructure: UseOracle = {UseOracle}", useOracle);
            logger.LogInformation("Infrastructure: UseOracleForSetup = {UseOracleForSetup}", useOracleForSetup);
            logger.LogInformation("Infrastructure: ConnectionString = {ConnectionString}", config.GetConnectionString("SqliteConnection"));

    string? connectionString = config.GetConnectionString("SqliteConnection");

    if (useOracle)
    {
      services.AddSingleton<Data.Oracle.IOracleConnectionFactory, Data.Oracle.OracleConnectionFactory>();
      services.AddSingleton<IDbConnectionFactory>(sp => (IDbConnectionFactory)sp.GetRequiredService<Data.Oracle.IOracleConnectionFactory>());
    }
    else
    {
      services.AddSingleton<Data.Sqlite.ISqliteConnectionFactory, Data.Sqlite.SqliteConnectionFactory>();
      services.AddSingleton<IDbConnectionFactory>(sp => (IDbConnectionFactory)sp.GetRequiredService<Data.Sqlite.ISqliteConnectionFactory>());

      // Keep EF DbContext only for seeding in SQLite mode
      Guard.Against.Null(connectionString);
      services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
    }

    // Register database-specific ContributorService implementations
    if (useOracle)
    {
      services.AddScoped<Relevo.Core.Interfaces.IContributorService, OracleContributorService>();
    }
    else
    {
      services.AddScoped<Relevo.Core.Interfaces.IContributorService, SqliteContributorService>();
    }

    if (useOracle)
    {
      services.AddScoped<IListContributorsQueryService, Data.Queries.OracleListContributorsQueryService>();
    }
    else
    {
      services.AddScoped<IListContributorsQueryService, ListContributorsQueryService>();
    }

    services.AddScoped<IDeleteContributorService, DeleteContributorService>();

    // Setup Repository Services (Hexagonal Architecture)
    if (useOracleForSetup)
    {
      services.AddScoped<ISetupRepository, Repositories.OracleSetupRepository>();
    }
    else
    {
      Guard.Against.Null(connectionString);
      services.AddScoped<ISetupRepository>(sp => new Repositories.SqliteSetupRepository(connectionString));
    }

    // Setup Use Cases
    services.AddScoped<Relevo.UseCases.Setup.AssignPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetMyPatientsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetMyHandoversUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetUnitsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetShiftsUseCase>();
    services.AddScoped<Relevo.UseCases.Setup.GetPatientsByUnitUseCase>();

    // Setup Application Service
    services.AddScoped<Relevo.Core.Interfaces.ISetupService, Relevo.UseCases.Setup.SetupService>();

    // Authentication and Authorization Services
    services.AddScoped<Relevo.Core.Interfaces.IAuthenticationService, Auth.ClerkAuthenticationService>();
    services.AddScoped<Relevo.Core.Interfaces.IAuthorizationService, Auth.ClerkAuthorizationService>();
    services.AddScoped<Relevo.Core.Interfaces.IUserContext, Auth.HttpContextUserContext>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
