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
using Oracle.ManagedDataAccess.Client;

namespace Relevo.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  public CustomWebApplicationFactory()
  {
    // Set environment variable before any host building occurs to ensure it's picked up by all parts of the application
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    var host = builder.Build();
    host.Start();

    // Skip database seeding for functional tests since we're using Oracle with Dapper
    // The database should be pre-seeded or tests should use mock data
    var serviceProvider = host.Services;
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      try
      {
        // Contributor seeding removed - functionality deleted
        logger.LogInformation("Contributor seeding skipped - functionality removed");
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
    builder
      .UseEnvironment("Testing")
      .ConfigureAppConfiguration(config =>
      {
        var integrationConfig = new ConfigurationBuilder()
          .AddInMemoryCollection(new Dictionary<string, string?>
          {
            { "UseOracle", "true" },
            { "UseOracleForSetup", "true" },
            { "ConnectionStrings:Oracle", "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15" },
            { "Oracle:ConnectionString", "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15" }
          })
          .Build();

        config.AddConfiguration(integrationConfig);
      })
      .ConfigureServices(services =>
      {
        // Replace the real authentication service with our test version
        var authenticationServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAuthenticationService));
        if (authenticationServiceDescriptor != null)
        {
          services.Remove(authenticationServiceDescriptor);
        }
        services.AddSingleton<IAuthenticationService, TestAuthenticationService>();
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

    // Extract user ID from token if it contains one, otherwise use a consistent test ID
    var userId = ExtractUserIdFromToken(token) ?? "user_2abcdefghijklmnop123456789"; // Clerk-like format

    // Create a test user for functional tests
    var user = new User
    {
      Id = userId,
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

  private string? ExtractUserIdFromToken(string token)
  {
    // Try to extract user ID from token if it's encoded
    // This simulates Clerk's JWT structure where user_id might be in the payload
    try
    {
      // Simple check for test-token- pattern
      if (token.StartsWith("test-token-"))
      {
        // Extract user ID from test token format like "test-token-user_123"
        var parts = token.Split('-');
        if (parts.Length >= 3)
        {
          return parts[2]; // e.g., "user_123" from "test-token-user_123"
        }
      }
    }
    catch
    {
      // If extraction fails, return null and use default
    }

    return null;
  }
}
