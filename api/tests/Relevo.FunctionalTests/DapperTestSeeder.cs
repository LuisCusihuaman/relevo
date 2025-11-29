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
        // Clean existing data AND DROP tables with schema changes
        var tablesToDrop = new[] {
            "HANDOVER_PATIENT_DATA", "HANDOVER_SITUATION_AWARENESS", "HANDOVER_SYNTHESIS",
            "HANDOVER_ACTIVITY_LOG", "HANDOVER_MENTIONS", "HANDOVER_MESSAGES",
            "HANDOVER_CONTINGENCY", "HANDOVER_CHECKLISTS", "HANDOVER_PARTICIPANTS",
            "HANDOVER_SYNC_STATUS", "HANDOVER_ACTION_ITEMS", 
            "HANDOVERS", "USER_ASSIGNMENTS"
        };

        foreach (var table in tablesToDrop) 
        {
            try { connection.Execute($"DROP TABLE {table} CASCADE CONSTRAINTS"); } catch (OracleException) {}
        }

        var tablesToDelete = new[] {
             "PATIENT_SUMMARIES", "PATIENTS", "UNITS", "SHIFTS", "USERS", "USER_PREFERENCES", "USER_SESSIONS"
        };
        foreach (var table in tablesToDelete) 
        {
            try { connection.Execute($"DELETE FROM {table}"); } catch (OracleException ex) when (ex.Number == 942 || ex.Number == 2292) {}
        }

        // Seed Users/Contributors to satisfy FKs if any. 
        // The error ORA-02291 indicates RESPONSIBLE_PHYSICIAN_ID FK violation and FK_HANDOVERS_ASSIGNMENT
        
        // Try to create USERS table just in case it is the target
        try { 
            connection.Execute(@"
                CREATE TABLE USERS (
                    ID VARCHAR2(255) NOT NULL,
                    EMAIL VARCHAR2(255),
                    FIRST_NAME VARCHAR2(100),
                    LAST_NAME VARCHAR2(100),
                    FULL_NAME VARCHAR2(255),
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    CONSTRAINT PK_USERS PRIMARY KEY (ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}
        
        // Create USER_ASSIGNMENTS table if not exists (Schema updated to match SQL definitions)
        try {
             connection.Execute(@"
                CREATE TABLE USER_ASSIGNMENTS (
                    ASSIGNMENT_ID VARCHAR2(255) NOT NULL,
                    USER_ID VARCHAR2(255) NOT NULL,
                    SHIFT_ID VARCHAR2(50) NOT NULL,
                    PATIENT_ID VARCHAR2(50) NOT NULL,
                    ASSIGNED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    CONSTRAINT PK_USER_ASSIGNMENTS PRIMARY KEY (ASSIGNMENT_ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Seed User
        try {
            connection.Execute(@"
                MERGE INTO USERS u
                USING (SELECT :Id as Id, :Email as Email, :FirstName as FirstName, :LastName as LastName, :FullName as FullName FROM DUAL) s
                ON (u.ID = s.Id)
                WHEN NOT MATCHED THEN
                INSERT (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES (s.Id, s.Email, s.FirstName, s.LastName, s.FullName)",
                new { Id = "dr-1", Email = "dr1@example.com", FirstName = "Doctor", LastName = "One", FullName = "Dr. One" });
        } catch (OracleException) {}

        // Seed Units
        try {
            connection.Execute(@"
                INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT) VALUES
                (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = "unit-1", Name = "UCI", Description = "Unidad de Cuidados Intensivos" });
        } catch (OracleException e) when (e.Number == 1) {}

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
        try {
            connection.Execute(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT) VALUES
                (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = "pat-001", Name = "María García", UnitId = "unit-1", DateOfBirth = new DateTime(2010, 1, 1), Gender = "Female", AdmissionDate = DateTime.Now.AddDays(-2), RoomNumber = "101", Diagnosis = "Neumonía" });
        } catch (OracleException e) when (e.Number == 1) {}
        
        try {
            connection.Execute(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT) VALUES
                (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = "pat-002", Name = "Carlos Rodríguez", UnitId = "unit-1", DateOfBirth = new DateTime(2012, 5, 15), Gender = "Male", AdmissionDate = DateTime.Now.AddDays(-1), RoomNumber = "201", Diagnosis = "Gastroenteritis" });
        } catch (OracleException e) when (e.Number == 1) {}

        // Seed Assignment (Moved after Users, Shifts, and Patients are seeded)
        try {
            connection.Execute(@"
                MERGE INTO USER_ASSIGNMENTS ua
                USING (SELECT :Id as Id, :UserId as UserId, :ShiftId as ShiftId, :PatientId as PatientId FROM DUAL) s
                ON (ua.ASSIGNMENT_ID = s.Id)
                WHEN NOT MATCHED THEN
                INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
                VALUES (s.Id, s.UserId, s.ShiftId, s.PatientId)",
                new { Id = "asn-001", UserId = "dr-1", ShiftId = "shift-day", PatientId = "pat-001" });
        } catch (OracleException) {}

        // Create HANDOVERS table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVERS (
                    ID VARCHAR2(50) NOT NULL,
                    ASSIGNMENT_ID VARCHAR2(255) NOT NULL,
                    PATIENT_ID VARCHAR2(50) NOT NULL,
                    STATUS VARCHAR2(20) DEFAULT 'Draft',
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    COMPLETED_AT TIMESTAMP,
                    SHIFT_NAME VARCHAR2(100),
                    FROM_SHIFT_ID VARCHAR2(50),
                    TO_SHIFT_ID VARCHAR2(50),
                    FROM_DOCTOR_ID VARCHAR2(255),
                    TO_DOCTOR_ID VARCHAR2(255),
                    RECEIVER_USER_ID VARCHAR2(255),
                    CREATED_BY VARCHAR2(255),
                    INITIATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    ACCEPTED_AT TIMESTAMP,
                    COMPLETED_BY VARCHAR2(255),
                    READY_AT TIMESTAMP,
                    STARTED_AT TIMESTAMP,
                    ACKNOWLEDGED_AT TIMESTAMP,
                    CANCELLED_AT TIMESTAMP,
                    REJECTED_AT TIMESTAMP,
                    REJECTION_REASON VARCHAR2(4000),
                    EXPIRED_AT TIMESTAMP,
                    HANDOVER_TYPE VARCHAR2(30),
                    HANDOVER_WINDOW_DATE DATE,
                    RESPONSIBLE_PHYSICIAN_ID VARCHAR2(255),
                    CONSTRAINT PK_HANDOVERS PRIMARY KEY (ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Create HANDOVER_PATIENT_DATA table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVER_PATIENT_DATA (
                    HANDOVER_ID VARCHAR2(50) NOT NULL,
                    ILLNESS_SEVERITY VARCHAR2(20) NOT NULL,
                    SUMMARY_TEXT CLOB,
                    LAST_EDITED_BY VARCHAR2(255),
                    STATUS VARCHAR2(20) DEFAULT 'draft',
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    CONSTRAINT PK_HANDOVER_PATIENT_DATA PRIMARY KEY (HANDOVER_ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Create HANDOVER_SYNTHESIS table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVER_SYNTHESIS (
                    HANDOVER_ID VARCHAR2(50) NOT NULL,
                    CONTENT CLOB,
                    LAST_EDITED_BY VARCHAR2(255),
                    STATUS VARCHAR2(20) DEFAULT 'draft',
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    CONSTRAINT PK_HANDOVER_SYNTHESIS PRIMARY KEY (HANDOVER_ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Create HANDOVER_ACTION_ITEMS table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVER_ACTION_ITEMS (
                    ID VARCHAR2(50) PRIMARY KEY,
                    HANDOVER_ID VARCHAR2(50) NOT NULL,
                    DESCRIPTION VARCHAR2(500) NOT NULL,
                    IS_COMPLETED NUMBER(1) DEFAULT 0, -- 0 = false, 1 = true
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    COMPLETED_AT TIMESTAMP,
                    CONSTRAINT FK_ACTION_ITEMS_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Create HANDOVER_PARTICIPANTS table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVER_PARTICIPANTS (
                    ID VARCHAR2(50) PRIMARY KEY,
                    HANDOVER_ID VARCHAR2(50) NOT NULL,
                    USER_ID VARCHAR2(255) NOT NULL,
                    USER_NAME VARCHAR2(200) NOT NULL,
                    USER_ROLE VARCHAR2(100),
                    STATUS VARCHAR2(20) DEFAULT 'active',
                    JOINED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    LAST_ACTIVITY TIMESTAMP DEFAULT SYSTIMESTAMP,
                    CONSTRAINT FK_PARTICIPANTS_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Create HANDOVER_SITUATION_AWARENESS table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVER_SITUATION_AWARENESS (
                    HANDOVER_ID VARCHAR2(50) PRIMARY KEY REFERENCES HANDOVERS(ID),
                    CONTENT CLOB,
                    LAST_EDITED_BY VARCHAR2(255) REFERENCES USERS(ID),
                    STATUS VARCHAR2(20) DEFAULT 'draft',
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Seed Handovers
        try {
            connection.Execute(@"
                INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, CREATED_AT, SHIFT_NAME, CREATED_BY, RESPONSIBLE_PHYSICIAN_ID, HANDOVER_TYPE) VALUES
                (:Id, :AssignmentId, :PatientId, :Status, SYSTIMESTAMP, :ShiftName, :CreatedBy, :ResponsiblePhysicianId, :HandoverType)",
                new { 
                    Id = "hvo-001", 
                    AssignmentId = "asn-001", 
                    PatientId = "pat-001", 
                    Status = "Draft", 
                    ShiftName = "Day", 
                    CreatedBy = "dr-1", 
                    ResponsiblePhysicianId = "dr-1",
                    HandoverType = "ShiftToShift"
                });
        } catch (OracleException e) 
        {
             // Console.WriteLine(e.Message); // Helpful for debug, but avoiding console spam in tool output
             if (e.Number != 1 && e.Number != 2291) throw; // Re-throw unexpected errors
        } 

        // Seed Handover Patient Data
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_PATIENT_DATA (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, LAST_EDITED_BY, STATUS, CREATED_AT, UPDATED_AT) VALUES
                (:HandoverId, :Severity, :Summary, :LastEditedBy, :Status, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    HandoverId = "hvo-001", 
                    Severity = "Stable", 
                    Summary = "Patient stable overnight",
                    LastEditedBy = "dr-1",
                    Status = "draft"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}
        
        // Seed Handover Situation Awareness
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_SITUATION_AWARENESS (HANDOVER_ID, CONTENT, LAST_EDITED_BY, STATUS, CREATED_AT, UPDATED_AT) VALUES
                (:HandoverId, :Content, :LastEditedBy, :Status, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    HandoverId = "hvo-001", 
                    Content = "Initial SA",
                    LastEditedBy = "dr-1",
                    Status = "draft"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Seed Action Items
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT) VALUES
                (:Id, :HandoverId, :Description, :IsCompleted, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    Id = "item-001", 
                    HandoverId = "hvo-001", 
                    Description = "Check blood pressure",
                    IsCompleted = 0
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Create PATIENT_SUMMARIES table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE PATIENT_SUMMARIES (
                    ID VARCHAR2(50) PRIMARY KEY,
                    PATIENT_ID VARCHAR2(50) NOT NULL,
                    PHYSICIAN_ID VARCHAR2(255) NOT NULL, -- Clerk User ID del médico asignado
                    SUMMARY_TEXT CLOB NOT NULL,
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    LAST_EDITED_BY VARCHAR2(255), -- Clerk User ID de quien editó por última vez
                    CONSTRAINT FK_PATIENT_SUMMARIES_PATIENT FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Seed Patient Summaries
        try {
            connection.Execute(@"
                INSERT INTO PATIENT_SUMMARIES (ID, PATIENT_ID, PHYSICIAN_ID, SUMMARY_TEXT, CREATED_AT, UPDATED_AT, LAST_EDITED_BY) VALUES
                (:Id, :PatientId, :PhysicianId, :SummaryText, SYSTIMESTAMP, SYSTIMESTAMP, :LastEditedBy)",
                new { 
                    Id = "sum-001", 
                    PatientId = "pat-001", 
                    PhysicianId = "dr-1", 
                    SummaryText = "Patient history...",
                    LastEditedBy = "dr-1"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Create HANDOVER_CONTINGENCY table if not exists
        try {
            connection.Execute(@"
                CREATE TABLE HANDOVER_CONTINGENCY (
                    ID VARCHAR2(50) PRIMARY KEY,
                    HANDOVER_ID VARCHAR2(50) NOT NULL,
                    CONDITION_TEXT VARCHAR2(1000),
                    ACTION_TEXT VARCHAR2(1000),
                    PRIORITY VARCHAR2(20),
                    STATUS VARCHAR2(20) DEFAULT 'active',
                    CREATED_BY VARCHAR2(255),
                    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
                    CONSTRAINT FK_CONTINGENCY_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)
                )");
        } catch (OracleException e) when (e.Number == 955) {}

        // Seed Contingency Plans
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT) VALUES
                (:Id, :HandoverId, :ConditionText, :ActionText, :Priority, :Status, :CreatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
                new {
                    Id = "plan-001",
                    HandoverId = "hvo-001",
                    ConditionText = "If BP drops below 90/60",
                    ActionText = "Administer fluids",
                    Priority = "High",
                    Status = "active",
                    CreatedBy = "dr-1"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}
    }
}
