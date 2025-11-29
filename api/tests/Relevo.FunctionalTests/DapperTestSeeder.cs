using Dapper;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace Relevo.FunctionalTests;

public class DapperTestSeeder(IConfiguration configuration)
{
    public void Seed()
    {
        string connectionString = configuration.GetConnectionString("OracleConnection")!;
        using var connection = new OracleConnection(connectionString);
        connection.Open();

        // Clean existing data
        try
        {
            connection.Execute("DELETE FROM CONTRIBUTORS");
        }
        catch (OracleException ex) when (ex.Number == 942) // Table doesn't exist
        {
            // Create Table
            connection.Execute(@"
                CREATE TABLE CONTRIBUTORS (
                    Id NUMBER(10) NOT NULL,
                    Name VARCHAR2(100) NOT NULL,
                    Status NUMBER(10) DEFAULT 0 NOT NULL,
                    PhoneNumber_CountryCode VARCHAR2(50),
                    PhoneNumber_Number VARCHAR2(50),
                    PhoneNumber_Extension VARCHAR2(50),
                    CONSTRAINT PK_CONTRIBUTORS PRIMARY KEY (Id)
                )");
        }

        // Reset Sequence (Optional but good for consistent IDs)
        // Oracle sequence reset is tricky, usually Drop/Create or Increment by difference.
        // For tests, we'll just force IDs if possible, or rely on sequence and just insert what we need.
        // Since we need specific IDs (1 and 2) for assertions, we should try to force them or reset.
        // However, Oracle sequences + explicit ID insert triggers can be messy.
        // Let's try direct insert first.

        // Seed Data matching SeedData.cs
        // "Ardalis" (ID 1)
        // "Snowfrog" (ID 2)
        
        // Note: In the App, we use a Sequence. To guarantee IDs 1 and 2, we might need to toggle identity insert (not available in 11g easily)
        // or just update them after insert.
        // Or drop/create the sequence.

        // Simple approach: Drop and recreate sequence to reset to 1
        try 
        {
            connection.Execute("DROP SEQUENCE RELEVO_APP.CONTRIBUTORS_SEQ");
        } 
        catch {} // Ignore if doesn't exist
        
        connection.Execute("CREATE SEQUENCE RELEVO_APP.CONTRIBUTORS_SEQ START WITH 1 INCREMENT BY 1");

        connection.Execute(@"
            INSERT INTO CONTRIBUTORS (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) 
            VALUES (CONTRIBUTORS_SEQ.NEXTVAL, :Name, 1, NULL, NULL, NULL)", 
            new { Name = TestSeeds.Contributor1 });

        connection.Execute(@"
            INSERT INTO CONTRIBUTORS (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) 
            VALUES (CONTRIBUTORS_SEQ.NEXTVAL, :Name, 1, NULL, NULL, NULL)", 
            new { Name = TestSeeds.Contributor2 });
    }
}
