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
      // Keep EF for write model if needed; otherwise can be removed later.
      string? sqlite = config.GetConnectionString("SqliteConnection");
      if (!string.IsNullOrWhiteSpace(sqlite))
      {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqlite));
      }
    }
    else
    {
      string? connectionString = config.GetConnectionString("SqliteConnection");
      Guard.Against.Null(connectionString);
      services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
    }

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
           .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
           .AddScoped<IListContributorsQueryService>(sp =>
           {
             var cfg = sp.GetRequiredService<IConfiguration>();
             bool useOracle = cfg.GetValue("UseOracle", false);
             if (useOracle)
             {
               return new Data.Queries.OracleListContributorsQueryService(
                 sp.GetRequiredService<Data.Oracle.IOracleConnectionFactory>());
             }
             return new ListContributorsQueryService(sp.GetRequiredService<AppDbContext>());
           })
           .AddScoped<IDeleteContributorService, DeleteContributorService>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
