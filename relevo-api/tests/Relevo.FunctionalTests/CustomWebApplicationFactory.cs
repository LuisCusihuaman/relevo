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
        // Try to seed using the Oracle connection directly
        var connectionFactory = scopedServices.GetRequiredService<IDbConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();
        SeedTestContributors(connection);
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

  private static void SeedTestContributors(IDbConnection connection)
  {
      Console.WriteLine("Starting test data seeding...");

      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
        Console.WriteLine("Database connection opened");
      }

      using var command = connection.CreateCommand();

      // Always try to create the view if handover tables exist
      try
      {
        using (var viewCmd = connection.CreateCommand())
        {
          viewCmd.CommandText = @"CREATE OR REPLACE VIEW VW_HANDOVERS_STATE AS SELECT h.ID as HandoverId, 'InProgress' AS StateName FROM HANDOVERS h";
          viewCmd.ExecuteNonQuery();
          Console.WriteLine("VW_HANDOVERS_STATE view created/updated");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"View creation failed: {ex.Message}");
      }

      // Always recreate tables since schema may have changed
      try
      {
        using (var dropCmd = connection.CreateCommand())
        {
          dropCmd.CommandText = "DROP TABLE PATIENTS";
          dropCmd.ExecuteNonQuery();
          Console.WriteLine("Dropped existing PATIENTS table");
        }
      }
      catch { /* Table might not exist */ }

      try
      {
        using (var dropCmd = connection.CreateCommand())
        {
          dropCmd.CommandText = "DROP TABLE UNITS";
          dropCmd.ExecuteNonQuery();
          Console.WriteLine("Dropped existing UNITS table");
        }
      }
      catch { /* Table might not exist */ }

      // Create UNITS table with correct schema
      try
      {
        using (var createCmd = connection.CreateCommand())
        {
          createCmd.CommandText = @"
            CREATE TABLE UNITS (
              ID VARCHAR2(50) PRIMARY KEY,
              NAME VARCHAR2(100) NOT NULL,
              DESCRIPTION VARCHAR2(500),
              CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
              UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
            )";
          createCmd.ExecuteNonQuery();
          Console.WriteLine("Created UNITS table");
        }
      }
      catch { /* Table might already exist */ }

      using var transaction = connection.BeginTransaction();
      try
      {
        // Always ensure clean state by clearing and reseeding
        // This prevents interference between tests
        using (var cmd = connection.CreateCommand())
        {
          cmd.Transaction = transaction;

          // UNITS table should already be created outside transaction

          // Clear tables (skip if they don't exist)
          try { cmd.CommandText = "DELETE FROM CONTRIBUTORS"; cmd.ExecuteNonQuery(); } catch { }
          try { cmd.CommandText = "DELETE FROM SHIFTS"; cmd.ExecuteNonQuery(); } catch { }

          cmd.CommandText = @"
            CREATE TABLE PATIENTS (
              ID VARCHAR2(50) PRIMARY KEY,
              NAME VARCHAR2(200) NOT NULL,
              UNIT_ID VARCHAR2(50),
              DATE_OF_BIRTH DATE,
              GENDER VARCHAR2(20),
              ADMISSION_DATE TIMESTAMP,
              ROOM_NUMBER VARCHAR2(20),
              DIAGNOSIS VARCHAR2(500),
              ALLERGIES VARCHAR2(1000),
              MEDICATIONS VARCHAR2(1000),
              NOTES VARCHAR2(1000),
              MRN VARCHAR2(20),
              CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
              UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
            )";
          cmd.ExecuteNonQuery();

          // Seed UNITS (only ID and NAME since that's what the table has)
          cmd.CommandText = "INSERT INTO UNITS (ID, NAME) VALUES ('unit-1', 'UCI')";
          cmd.ExecuteNonQuery();

          cmd.CommandText = "INSERT INTO UNITS (ID, NAME) VALUES ('unit-2', 'Pediatría General')";
          cmd.ExecuteNonQuery();

          cmd.CommandText = "INSERT INTO UNITS (ID, NAME) VALUES ('unit-3', 'Pediatría Especializada')";
          cmd.ExecuteNonQuery();

          // Seed SHIFTS
          cmd.CommandText = "INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-day', 'Mañana', '07:00', '15:00')";
          cmd.ExecuteNonQuery();

          cmd.CommandText = "INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-night', 'Noche', '19:00', '07:00')";
          cmd.ExecuteNonQuery();

          // Seed PATIENTS (35 patients total as expected by tests)
          // UCI (unit-1) - 12 patients
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ROOM_NUMBER, DIAGNOSIS) VALUES ('pat-001', 'María García', 'unit-1', TO_DATE('2010-01-01', 'YYYY-MM-DD'), 'Female', '101', 'Neumonía')";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-002', 'Carlos Rodríguez', 'unit-1', TO_DATE('2010-01-02', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-003', 'Ana López', 'unit-1', TO_DATE('2010-01-03', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-004', 'Miguel Hernández', 'unit-1', TO_DATE('2010-01-04', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-005', 'Isabella González', 'unit-1', TO_DATE('2010-01-05', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-006', 'David Pérez', 'unit-1', TO_DATE('2010-01-06', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-007', 'Sofia Martínez', 'unit-1', TO_DATE('2010-01-07', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-008', 'José Sánchez', 'unit-1', TO_DATE('2010-01-08', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-009', 'Carmen Díaz', 'unit-1', TO_DATE('2010-01-09', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-010', 'Antonio Moreno', 'unit-1', TO_DATE('2010-01-10', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-011', 'Elena Jiménez', 'unit-1', TO_DATE('2010-01-11', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-012', 'Francisco Ruiz', 'unit-1', TO_DATE('2010-01-12', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();

          // Pediatría General (unit-2) - 12 patients
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-013', 'Lucía Álvarez', 'unit-2', TO_DATE('2012-01-13', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-014', 'Pablo Romero', 'unit-2', TO_DATE('2012-01-14', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-015', 'Valentina Navarro', 'unit-2', TO_DATE('2012-01-15', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-016', 'Diego Torres', 'unit-2', TO_DATE('2012-01-16', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-017', 'Marta Ramírez', 'unit-2', TO_DATE('2012-01-17', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-018', 'Adrián Gil', 'unit-2', TO_DATE('2012-01-18', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-019', 'Clara Serrano', 'unit-2', TO_DATE('2012-01-19', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-020', 'Hugo Castro', 'unit-2', TO_DATE('2012-01-20', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-021', 'Natalia Rubio', 'unit-2', TO_DATE('2012-01-21', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-022', 'Iván Ortega', 'unit-2', TO_DATE('2012-01-22', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-023', 'Paula Delgado', 'unit-2', TO_DATE('2012-01-23', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-024', 'Mario Guerrero', 'unit-2', TO_DATE('2012-01-24', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();

          // Pediatría Especializada (unit-3) - 11 patients
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-025', 'Laura Flores', 'unit-3', TO_DATE('2008-01-25', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-026', 'Álvaro Vargas', 'unit-3', TO_DATE('2008-01-26', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-027', 'Cristina Medina', 'unit-3', TO_DATE('2008-01-27', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-028', 'Sergio Herrera', 'unit-3', TO_DATE('2008-01-28', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-029', 'Alicia Castro', 'unit-3', TO_DATE('2008-01-29', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-030', 'Roberto Vega', 'unit-3', TO_DATE('2008-01-30', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-031', 'Beatriz León', 'unit-3', TO_DATE('2008-01-31', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-032', 'Manuel Peña', 'unit-3', TO_DATE('2008-02-01', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-033', 'Silvia Cortés', 'unit-3', TO_DATE('2008-02-02', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-034', 'Fernando Aguilar', 'unit-3', TO_DATE('2008-02-03', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();
          cmd.CommandText = "INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH) VALUES ('pat-035', 'Teresa Santana', 'unit-3', TO_DATE('2008-02-04', 'YYYY-MM-DD'))";
          cmd.ExecuteNonQuery();

          // Create CONTRIBUTORS table if it doesn't exist
          try
          {
            cmd.CommandText = @"
              CREATE TABLE CONTRIBUTORS (
                ID NUMBER PRIMARY KEY,
                NAME VARCHAR2(200) NOT NULL,
                EMAIL VARCHAR2(200) NOT NULL,
                PHONE_NUMBER VARCHAR2(20),
                CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
              )";
            cmd.ExecuteNonQuery();
          }
          catch { /* Table might already exist */ }

          // Create USERS table for handover functionality
          try
          {
            cmd.CommandText = @"
              CREATE TABLE USERS (
                ID VARCHAR2(255) PRIMARY KEY,
                EMAIL VARCHAR2(255),
                FIRST_NAME VARCHAR2(100),
                LAST_NAME VARCHAR2(100),
                FULL_NAME VARCHAR2(200),
                ROLES VARCHAR2(500),
                PERMISSIONS VARCHAR2(1000),
                LAST_LOGIN_AT TIMESTAMP,
                IS_ACTIVE NUMBER(1) DEFAULT 1,
                CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
              )";
            cmd.ExecuteNonQuery();
          }
          catch { /* Table might already exist */ }

          // Create HANDOVERS table
          try
          {
            cmd.CommandText = @"
              CREATE TABLE HANDOVERS (
                ID VARCHAR2(50) PRIMARY KEY,
                ASSIGNMENT_ID VARCHAR2(255),
                PATIENT_ID VARCHAR2(50),
                STATUS VARCHAR2(20) DEFAULT 'Draft',
                ILLNESS_SEVERITY VARCHAR2(20),
                PATIENT_SUMMARY VARCHAR2(1000),
                SITUATION_AWARENESS_DOC_ID VARCHAR2(100),
                SYNTHESIS VARCHAR2(1000),
                SHIFT_NAME VARCHAR2(100),
                CREATED_BY VARCHAR2(255),
                TO_DOCTOR_ID VARCHAR2(255),
                RECEIVER_USER_ID VARCHAR2(255),
                CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                READY_AT TIMESTAMP,
                STARTED_AT TIMESTAMP,
                ACKNOWLEDGED_AT TIMESTAMP,
                ACCEPTED_AT TIMESTAMP,
                COMPLETED_AT TIMESTAMP,
                CANCELLED_AT TIMESTAMP,
                REJECTED_AT TIMESTAMP,
                REJECTION_REASON VARCHAR2(4000),
                EXPIRED_AT TIMESTAMP,
                HANDOVER_TYPE VARCHAR2(30),
                STATE_NAME VARCHAR2(50) DEFAULT 'Draft',
                HANDOVER_WINDOW_DATE DATE,
                FROM_SHIFT_ID VARCHAR2(50),
                TO_SHIFT_ID VARCHAR2(50)
              )";
            cmd.ExecuteNonQuery();
          }
          catch { /* Table might already exist */ }

          // Create HANDOVER_ACTION_ITEMS table
          try
          {
            cmd.CommandText = @"
              CREATE TABLE HANDOVER_ACTION_ITEMS (
                ID VARCHAR2(50) PRIMARY KEY,
                HANDOVER_ID VARCHAR2(50),
                DESCRIPTION VARCHAR2(500),
                IS_COMPLETED NUMBER(1) DEFAULT 0,
                CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)
              )";
            cmd.ExecuteNonQuery();
          }
          catch { /* Table might already exist */ }

          // Seed USERS
          cmd.CommandText = @"
            INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, ROLES, PERMISSIONS, IS_ACTIVE)
            VALUES ('user_2abcdefghijklmnop123456789', 'test@example.com', 'Test', 'User', 'Test User', 'clinician', 'patients.read,patients.assign', 1)";
          cmd.ExecuteNonQuery();

          cmd.CommandText = @"
            INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, ROLES, PERMISSIONS, IS_ACTIVE)
            VALUES ('user-doctor1', 'doctor1@example.com', 'John', 'Doctor', 'John Doctor', 'clinician', 'patients.read,patients.assign', 1)";
          cmd.ExecuteNonQuery();

          // Seed USER_ASSIGNMENTS
          cmd.CommandText = "INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT) VALUES ('user_2abcdefghijklmnop123456789', 'shift-day', 'pat-001', SYSTIMESTAMP)";
          cmd.ExecuteNonQuery();

          // Seed HANDOVERS
          cmd.CommandText = @"
            INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SHIFT_NAME, CREATED_BY, TO_DOCTOR_ID, RECEIVER_USER_ID, STATE_NAME, FROM_SHIFT_ID, TO_SHIFT_ID)
            VALUES ('handover-001', 'assign-001', 'pat-001', 'InProgress', 'Stable', 'Patient is stable post-surgery with good vital signs.', 'Mañana', 'user-doctor1', 'user_2abcdefghijklmnop123456789', 'user_2abcdefghijklmnop123456789', 'InProgress', 'shift-day', 'shift-night')";
          cmd.ExecuteNonQuery();

          // Seed singleton tables directly (replaces the need for 09-backfill-singleton-tables.sql)
          cmd.CommandText = @"
            INSERT INTO HANDOVER_PATIENT_DATA (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, LAST_EDITED_BY, STATUS, CREATED_AT, UPDATED_AT)
            VALUES ('handover-001', 'Stable', TO_CLOB('Patient is stable post-surgery with good vital signs.'), 'user-doctor1', 'completed', SYSTIMESTAMP, SYSTIMESTAMP)";
          cmd.ExecuteNonQuery();

          // Seed HANDOVER_ACTION_ITEMS
          cmd.CommandText = @"
            INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
            VALUES ('action-001', 'handover-001', 'Monitor vital signs every 4 hours', 0)";
          cmd.ExecuteNonQuery();

          cmd.CommandText = @"
            INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
            VALUES ('action-002', 'handover-001', 'Administer pain medication as needed', 1)";
          cmd.ExecuteNonQuery();

          // Seed CONTRIBUTORS
          cmd.CommandText = @"
            INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL, PHONE_NUMBER, CREATED_AT, UPDATED_AT)
            VALUES (1, 'Ardalis', 'ardalis@test.com', '+1-555-0101', SYSTIMESTAMP, SYSTIMESTAMP)";
          cmd.ExecuteNonQuery();
        }

        transaction.Commit();

        Console.WriteLine("Successfully seeded test data");

        // Verify patient count
        using (var verifyCmd = connection.CreateCommand())
        {
          verifyCmd.CommandText = "SELECT COUNT(*) FROM PATIENTS";
          var count = Convert.ToInt32(verifyCmd.ExecuteScalar());
          Console.WriteLine($"Patient count after seeding: {count}");
        }
      }
      catch (Exception ex)
      {
        // Log the error instead of ignoring it
        Console.WriteLine($"Error seeding test data: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        transaction?.Rollback();
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
