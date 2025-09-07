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

  public CustomWebApplicationFactory()
  {
    // Set environment variable before any host building occurs to ensure it's picked up by all parts of the application
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    var host = builder.Build();
    host.Start();

    var serviceProvider = host.Services;
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();
      var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      db.Database.EnsureDeleted();
      db.Database.EnsureCreated();

      try
      {
        SeedTestContributors(db);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
      }
    }

    return host;
  }
  
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    _testDbFile = $"test_{Guid.NewGuid()}.db";

    builder
      .UseEnvironment("Testing")
      .ConfigureAppConfiguration(config =>
      {
        var integrationConfig = new ConfigurationBuilder()
          .AddInMemoryCollection(new Dictionary<string, string?>
          {
            { "UseOracle", "false" },
            { "UseOracleForSetup", "false" },
            { "ConnectionStrings:SqliteConnection", $"Data Source={_testDbFile}" }
          })
          .Build();

        config.AddConfiguration(integrationConfig);
      })
      .ConfigureServices((builderContext, services) =>
       {
         var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
         if (descriptor != null)
         {
           services.Remove(descriptor);
         }
         services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={_testDbFile}"));
       });
  }

  private static void SeedTestContributors(AppDbContext db)
  {
      var connection = db.Database.GetDbConnection();
      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
      }

      using var command = connection.CreateCommand();

      command.CommandText = "DELETE FROM Contributors";
      command.ExecuteNonQuery();

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
