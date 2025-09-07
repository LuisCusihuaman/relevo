using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    // context (AppDbContext).
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      // Reset Sqlite database for each test run
      // If using a real database, you'll likely want to remove this step.
      db.Database.EnsureDeleted();

      // Ensure the database is created.
      db.Database.EnsureCreated();

      try
      {
        // Can also skip creating the items
        //if (!db.ToDoItems.Any())
        //{
        // Seed the database with test data.
        SeedData.PopulateTestDataAsync(db).Wait();
        //}
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {exceptionMessage}", ex.Message);
      }
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
        .ConfigureAppConfiguration(config =>
        {
          // Override configuration for tests to use SQLite instead of Oracle
          config.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["UseOracle"] = "false",
            ["UseOracleForSetup"] = "false"
          });
        })
        .ConfigureServices(services =>
        {
          // Configure test dependencies here

          // Replace the authentication service with a test implementation
          var authServiceDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(Relevo.Core.Interfaces.IAuthenticationService));

          if (authServiceDescriptor != null)
          {
            services.Remove(authServiceDescriptor);
          }

          services.AddScoped<Relevo.Core.Interfaces.IAuthenticationService, TestAuthenticationService>();

          //// Remove the app's ApplicationDbContext registration.
          //var descriptor = services.SingleOrDefault(
          //d => d.ServiceType ==
          //    typeof(DbContextOptions<AppDbContext>));

          //if (descriptor != null)
          //{
          //  services.Remove(descriptor);
          //}

          //// This should be set for each individual test run
          //string inMemoryCollectionName = Guid.NewGuid().ToString();

          //// Add ApplicationDbContext using an in-memory database for testing.
          //services.AddDbContext<AppDbContext>(options =>
          //{
          //  options.UseInMemoryDatabase(inMemoryCollectionName);
          //});
        });
  }
}

/// <summary>
/// Test implementation of IAuthenticationService for functional testing
/// </summary>
public class TestAuthenticationService : IAuthenticationService
{
  public Task<AuthenticationResult> AuthenticateAsync(string token)
  {
    // For functional tests, accept any non-empty token as valid
    if (string.IsNullOrEmpty(token))
    {
      return Task.FromResult(AuthenticationResult.Failure("Token is required"));
    }

    // Create a test user for functional tests
    var user = new User
    {
      Id = "test-user-id",
      Email = "test@example.com",
      FirstName = "Test",
      LastName = "User",
      Roles = new[] { "clinician" },
      Permissions = new[] { "patients.read", "patients.assign" },
      LastLoginAt = DateTime.UtcNow,
      IsActive = true
    };

    return Task.FromResult(AuthenticationResult.Success(user));
  }

  public Task<bool> ValidateTokenAsync(string token)
  {
    // For functional tests, any non-empty token is considered valid
    return Task.FromResult(!string.IsNullOrEmpty(token));
  }
}
