using Relevo.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Relevo.FunctionalTests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Relevo.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  /// <summary>
  /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
  /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Development"); // will not send real emails
    var host = builder.Build();
    host.Start();

    // Get service provider.
    var serviceProvider = host.Services;

    // Create a scope to obtain a reference to the database
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var config = scopedServices.GetRequiredService<IConfiguration>();
      var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      var seeder = new DapperTestSeeder(config);
      seeder.Seed();
    }

    return host;
  }

  public HttpClient CreateAuthenticatedClient(string userId = "dr-1")
  {
      var client = CreateClient();
      client.DefaultRequestHeaders.Authorization = 
          new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userId);
      return client;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureAppConfiguration((context, config) =>
    {
        var appAssembly = typeof(CustomWebApplicationFactory<TProgram>).Assembly;
        var appPath = Path.GetDirectoryName(appAssembly.Location);
        if (appPath != null)
        {
            config.AddJsonFile(Path.Combine(appPath, "appsettings.json"), optional: true);
        }
    });

    builder
        .ConfigureServices(services =>
        {
          // Remove real Clerk authentication configuration
          var descriptors = services.Where(d => 
              d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>)).ToList();
          
          foreach (var d in descriptors)
          {
              services.Remove(d);
          }
          
          // Add test authentication
          services.AddAuthentication(TestAuthHandler.SchemeName)
              .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                  TestAuthHandler.SchemeName, null);
        });
  }
}
