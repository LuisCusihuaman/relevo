using Ardalis.ListStartupServices;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Configuration;

namespace Relevo.Web.Configurations;

public static class WebApplicationConfigs
{
  public static async Task<IApplicationBuilder> UseAppMiddleware(this WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
      app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
    }
    else
    {
      app.UseDefaultExceptionHandler(); // from FastEndpoints
      app.UseHsts();
    }

    app.UseFastEndpoints()
        .UseSwaggerGen(); // Includes AddFileServer and static files middleware

    app.UseHttpsRedirection();

    await SeedDatabase(app);

    return app;
  }

  static async Task SeedDatabase(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
      // Always use Oracle now
      var contributorService = services.GetRequiredService<IContributorService>();
      await SeedOracleData(contributorService, logger);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
    }
  }

  static async Task SeedOracleData(IContributorService contributorService, ILogger logger)
  {
    // Check if data already exists (basic check - we could implement a more sophisticated check)
    var contributors = await contributorService.GetAllAsync();
    if (contributors.Any())
    {
      logger.LogInformation("Database already seeded, skipping Oracle seeding");
      return;
    }

    // Add test data using Dapper
    var contributor1 = new Contributor("Ardalis");
    var contributor2 = new Contributor("Snowfrog");

    await contributorService.CreateAsync(contributor1);
    await contributorService.CreateAsync(contributor2);

    logger.LogInformation("Oracle database seeded with test data");
  }
}
