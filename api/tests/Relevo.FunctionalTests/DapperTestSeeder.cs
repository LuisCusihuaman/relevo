using Dapper;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Relevo.FunctionalTests;

public class DapperTestSeeder(IConfiguration configuration)
{
    public void Seed()
    {
        string connectionString = configuration.GetConnectionString("OracleConnection")!;
        using var connection = new OracleConnection(connectionString);
        connection.Open();

        SeedContributors(connection);
        SeedUnitsAndPatients(connection);
    }

    private void SeedContributors(IDbConnection connection)
    {
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

        // Reset Sequence
        try 
        {
            connection.Execute("DROP SEQUENCE RELEVO_APP.CONTRIBUTORS_SEQ");
        } 
        catch {} 
        
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

    private void SeedUnitsAndPatients(IDbConnection connection)
    {
        // Clean existing data
        var tablesToDelete = new[] {
            "HANDOVER_PATIENT_DATA", "HANDOVER_SITUATION_AWARENESS", "HANDOVER_SYNTHESIS",
            "HANDOVER_ACTIVITY_LOG", "HANDOVER_MENTIONS", "HANDOVER_MESSAGES",
            "HANDOVER_CONTINGENCY", "HANDOVER_CHECKLISTS", "HANDOVER_PARTICIPANTS",
            "HANDOVER_SYNC_STATUS", "HANDOVER_ACTION_ITEMS", 
            "HANDOVERS", "USER_ASSIGNMENTS", "PATIENT_SUMMARIES", "PATIENTS", "UNITS"
        };

        foreach (var table in tablesToDelete) 
        {
            try { connection.Execute($"DELETE FROM {table}"); } catch (OracleException ex) when (ex.Number == 942) {}
        }

        // Create Tables if they don't exist
        try { 
            connection.Execute(@"
                CREATE TABLE UNITS (
                    ID VARCHAR2(50) NOT NULL,
                    NAME VARCHAR2(100) NOT NULL,
                    DESCRIPTION VARCHAR2(255),
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    CONSTRAINT PK_UNITS PRIMARY KEY (ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {} // Name used by existing object

            try {
            connection.Execute(@"
                CREATE TABLE PATIENTS (
                    ID VARCHAR2(50) NOT NULL,
                    NAME VARCHAR2(100) NOT NULL,
                    UNIT_ID VARCHAR2(50) NOT NULL,
                    DATE_OF_BIRTH DATE,
                    GENDER VARCHAR2(20),
                    ADMISSION_DATE DATE,
                    ROOM_NUMBER VARCHAR2(20),
                    DIAGNOSIS VARCHAR2(255),
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    CONSTRAINT PK_PATIENTS PRIMARY KEY (ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Seed Units
        connection.Execute(@"
            INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT) VALUES
            (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = "unit-1", Name = "UCI", Description = "Unidad de Cuidados Intensivos" });

        // Create SHIFTS table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE SHIFTS (
                    ID VARCHAR2(50) NOT NULL,
                    NAME VARCHAR2(100) NOT NULL,
                    START_TIME VARCHAR2(5) NOT NULL,
                    END_TIME VARCHAR2(5) NOT NULL,
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    CONSTRAINT PK_SHIFTS PRIMARY KEY (ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Clean SHIFTS
        connection.Execute("DELETE FROM SHIFTS");

        // Seed Shifts
        connection.Execute(@"
            INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT) VALUES
            (:Id, :Name, :StartTime, :EndTime, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = "shift-day", Name = "Mañana", StartTime = "07:00", EndTime = "15:00" });

        connection.Execute(@"
            INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT) VALUES
            (:Id, :Name, :StartTime, :EndTime, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = "shift-night", Name = "Noche", StartTime = "19:00", EndTime = "07:00" });

        // Seed Patients
        connection.Execute(@"
            INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT) VALUES
            (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = "pat-001", Name = "María García", UnitId = "unit-1", DateOfBirth = new DateTime(2010, 1, 1), Gender = "Female", AdmissionDate = DateTime.Now.AddDays(-2), RoomNumber = "101", Diagnosis = "Neumonía" });
        
        connection.Execute(@"
            INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT) VALUES
            (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = "pat-002", Name = "Carlos Rodríguez", UnitId = "unit-1", DateOfBirth = new DateTime(2012, 5, 15), Gender = "Male", AdmissionDate = DateTime.Now.AddDays(-1), RoomNumber = "201", Diagnosis = "Gastroenteritis" });
    }
}
