using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using Relevo.Infrastructure.Data;
using Relevo.Infrastructure.Data.Queries;
using Relevo.UseCases.Contributors.List;
using Microsoft.EntityFrameworkCore;
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
    bool useOracle = config.GetValue("UseOracle", false);

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
      string? connectionString = config.GetConnectionString("SqliteConnection");
      Guard.Against.Null(connectionString);
      services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
    }

    // Register Dapper-based services
    services.AddScoped<Relevo.Core.Interfaces.IContributorService, ContributorService>();

    if (useOracle)
    {
      services.AddScoped<IListContributorsQueryService, Data.Queries.OracleListContributorsQueryService>();
    }
    else
    {
      services.AddScoped<IListContributorsQueryService, ListContributorsQueryService>();
    }

    services.AddScoped<IDeleteContributorService, DeleteContributorService>();

    // Authentication and Authorization Services
    services.AddScoped<Relevo.Core.Interfaces.IAuthenticationService, Auth.ClerkAuthenticationService>();
    services.AddScoped<Relevo.Core.Interfaces.IAuthorizationService, Auth.ClerkAuthorizationService>();
    services.AddScoped<Relevo.Core.Interfaces.IUserContext, Auth.HttpContextUserContext>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
