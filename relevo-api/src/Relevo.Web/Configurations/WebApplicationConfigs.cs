using Ardalis.ListStartupServices;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Configuration;

namespace Relevo.Web.Configurations;

public static class WebApplicationConfigs
{
  public static IApplicationBuilder UseAppMiddleware(this WebApplication app)
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

    SeedDatabase(app);

    return app;
  }

  static void SeedDatabase(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
      // Database seeding removed - contributors functionality deleted
      logger.LogInformation("Database seeding skipped - contributor functionality removed");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred during database initialization. {exceptionMessage}", ex.Message);
    }
  }

}
