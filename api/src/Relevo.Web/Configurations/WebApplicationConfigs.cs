using Ardalis.ListStartupServices;
using FastEndpoints;
using FastEndpoints.Swagger;

namespace Relevo.Web.Configurations;

public static class WebApplicationConfigs
{
  public static Task<IApplicationBuilder> UseAppMiddleware(this WebApplication app)
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

    return Task.FromResult<IApplicationBuilder>(app);
  }
}
