using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.Infrastructure;
using Relevo.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Extensions.Logging;
using System.IO;
using System;

namespace Relevo.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  private string? _testDbFile;

    /// <summary>
    /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
    /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected override IHost CreateHost(IHostBuilder builder)
    {
      builder.UseEnvironment("Development"); // will not send real emails

      // Clean up any existing test database
      if (!string.IsNullOrEmpty(_testDbFile) && File.Exists(_testDbFile))
      {
        File.Delete(_testDbFile);
      }

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

      // Ensure the database is created with default schema
      db.Database.EnsureCreated();

      // Seed test data directly using Dapper for predictable results
      SeedTestContributorsAsync(db);

      try
      {
        // Additional seeding if needed
        // SeedData.PopulateTestDataAsync(db).Wait();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {exceptionMessage}", ex.Message);
      }
    }

    return host;
  }

  /// <summary>
  /// Seed test contributors directly using Dapper
  /// </summary>
  private static void SeedTestContributorsAsync(AppDbContext db)
  {
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
      connection.Open();
    }

    using var command = connection.CreateCommand();

    // Clear existing data
    command.CommandText = "DELETE FROM Contributors";
    command.ExecuteNonQuery();

    // Insert test data with the correct column names for EF Core schema
    var insertCommands = new[]
    {
      "INSERT INTO Contributors (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) VALUES (1, 'Ardalis', 0, '', '+1-555-0101', '')",
      "INSERT INTO Contributors (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) VALUES (2, 'Snowfrog', 0, '', '+1-555-0102', '')"
    };

    foreach (var insertCmd in insertCommands)
    {
      command.CommandText = insertCmd;
      command.ExecuteNonQuery();
    }
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      // Generate unique database file name for this test run
      _testDbFile = $"test_{Guid.NewGuid()}.db";

      builder
        .ConfigureAppConfiguration(config =>
        {
          // Override configuration for tests to use SQLite instead of Oracle
          config.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["UseOracle"] = "false",
            ["UseOracleForSetup"] = "false",
            ["ConnectionStrings:SqliteConnection"] = $"Data Source={_testDbFile}"
          });
        })
        .ConfigureServices((context, services) =>
        {
          // Configure test dependencies here

          // Add infrastructure services (this should register the AppDbContext for SQLite)
          // Create a ConfigurationManager with test settings
          var configManager = new Microsoft.Extensions.Configuration.ConfigurationManager();
          configManager.AddConfiguration(context.Configuration);
          // Create a simple logger for the infrastructure services registration
          using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
          var logger = loggerFactory.CreateLogger<CustomWebApplicationFactory<TProgram>>();
          services.AddInfrastructureServices(configManager, logger);

          // Replace the authentication service with a test implementation
          var authServiceDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(Relevo.Core.Interfaces.IAuthenticationService));

          if (authServiceDescriptor != null)
          {
            services.Remove(authServiceDescriptor);
          }

          services.AddScoped<Relevo.Core.Interfaces.IAuthenticationService, TestAuthenticationService>();
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
