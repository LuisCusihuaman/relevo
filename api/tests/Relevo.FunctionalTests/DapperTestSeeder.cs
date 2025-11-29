using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Relevo.FunctionalTests;

public class DapperTestSeeder(IConfiguration configuration)
{
    // Unique prefix for this test run to avoid conflicts with parallel tests
    private static readonly string TestRunId = Guid.NewGuid().ToString()[..8];
    
    // Public test IDs that tests can reference
    public static string ContributorId1 => $"c1-{TestRunId}";
    public static string ContributorId2 => $"c2-{TestRunId}";
    public static string UnitId => $"unit-{TestRunId}";
    public static string ShiftDayId => $"shift-day-{TestRunId}";
    public static string ShiftNightId => $"shift-night-{TestRunId}";
    public static string PatientId1 => $"pat-001-{TestRunId}";
    public static string PatientId2 => $"pat-002-{TestRunId}";
    public static string UserId => $"dr-{TestRunId}";
    public static string AssignmentId => $"asn-{TestRunId}";
    public static string HandoverId => $"hvo-{TestRunId}";
    public static string ActionItemId => $"item-{TestRunId}";
    public static string SummaryId => $"sum-{TestRunId}";
    public static string ContingencyPlanId => $"plan-{TestRunId}";

    public void Seed()
    {
        string connectionString = configuration.GetConnectionString("OracleConnection")!;
        using var connection = new OracleConnection(connectionString);
        connection.Open();

        SeedTestData(connection);
    }

    private void SeedTestData(IDbConnection connection)
    {
        // Insert test user
        try {
            connection.Execute(@"
                INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES (:Id, :Email, :FirstName, :LastName, :FullName)",
                new { Id = UserId, Email = $"dr-{TestRunId}@example.com", FirstName = "Doctor", LastName = TestRunId, FullName = $"Dr. {TestRunId}" });
        } catch (OracleException e) when (e.Number == 1) {} // Unique constraint

        // Insert test unit
        try {
            connection.Execute(@"
                INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = UnitId, Name = $"UCI-{TestRunId}", Description = "Test Unit" });
        } catch (OracleException e) when (e.Number == 1) {}

        // Insert test shifts
        try {
            connection.Execute(@"
                INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :StartTime, :EndTime, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = ShiftDayId, Name = $"Day-{TestRunId}", StartTime = "07:00", EndTime = "15:00" });
        } catch (OracleException e) when (e.Number == 1) {}

        try {
            connection.Execute(@"
                INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :StartTime, :EndTime, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = ShiftNightId, Name = $"Night-{TestRunId}", StartTime = "19:00", EndTime = "07:00" });
        } catch (OracleException e) when (e.Number == 1) {}

        // Insert test patients
        try {
            connection.Execute(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = PatientId1, Name = $"Patient1-{TestRunId}", UnitId = UnitId, DateOfBirth = new DateTime(2010, 1, 1), Gender = "Female", AdmissionDate = DateTime.Now.AddDays(-2), RoomNumber = "101", Diagnosis = "Test Diagnosis" });
        } catch (OracleException e) when (e.Number == 1) {}

        try {
            connection.Execute(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = PatientId2, Name = $"Patient2-{TestRunId}", UnitId = UnitId, DateOfBirth = new DateTime(2012, 5, 15), Gender = "Male", AdmissionDate = DateTime.Now.AddDays(-1), RoomNumber = "201", Diagnosis = "Test Diagnosis 2" });
        } catch (OracleException e) when (e.Number == 1) {}

        // Insert test assignment
        try {
            connection.Execute(@"
                INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
                VALUES (:Id, :UserId, :ShiftId, :PatientId, SYSTIMESTAMP)",
                new { Id = AssignmentId, UserId = UserId, ShiftId = ShiftDayId, PatientId = PatientId1 });
        } catch (OracleException e) when (e.Number == 1) {}

        // Insert test handover
        try {
            connection.Execute(@"
                INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, CREATED_AT, SHIFT_NAME, CREATED_BY, RESPONSIBLE_PHYSICIAN_ID, HANDOVER_TYPE)
                VALUES (:Id, :AssignmentId, :PatientId, :Status, SYSTIMESTAMP, :ShiftName, :CreatedBy, :ResponsiblePhysicianId, :HandoverType)",
                new { 
                    Id = HandoverId, 
                    AssignmentId = AssignmentId, 
                    PatientId = PatientId1, 
                    Status = "Draft", 
                    ShiftName = "Day", 
                    CreatedBy = UserId, 
                    ResponsiblePhysicianId = UserId,
                    HandoverType = "ShiftToShift"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Insert test handover patient data
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_PATIENT_DATA (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, LAST_EDITED_BY, STATUS, CREATED_AT, UPDATED_AT)
                VALUES (:HandoverId, :Severity, :Summary, :LastEditedBy, :Status, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    HandoverId = HandoverId, 
                    Severity = "Stable", 
                    Summary = "Patient stable overnight",
                    LastEditedBy = UserId,
                    Status = "draft"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Insert test handover situation awareness
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_SITUATION_AWARENESS (HANDOVER_ID, CONTENT, LAST_EDITED_BY, STATUS, CREATED_AT, UPDATED_AT)
                VALUES (:HandoverId, :Content, :LastEditedBy, :Status, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    HandoverId = HandoverId, 
                    Content = "Initial SA",
                    LastEditedBy = UserId,
                    Status = "draft"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Insert test action item
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :HandoverId, :Description, :IsCompleted, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    Id = ActionItemId, 
                    HandoverId = HandoverId, 
                    Description = "Check blood pressure",
                    IsCompleted = 0
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Insert test patient summary
        try {
            connection.Execute(@"
                INSERT INTO PATIENT_SUMMARIES (ID, PATIENT_ID, PHYSICIAN_ID, SUMMARY_TEXT, CREATED_AT, UPDATED_AT, LAST_EDITED_BY)
                VALUES (:Id, :PatientId, :PhysicianId, :SummaryText, SYSTIMESTAMP, SYSTIMESTAMP, :LastEditedBy)",
                new { 
                    Id = SummaryId, 
                    PatientId = PatientId1, 
                    PhysicianId = UserId, 
                    SummaryText = "Patient history...",
                    LastEditedBy = UserId
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Insert test contingency plan
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :HandoverId, :ConditionText, :ActionText, :Priority, :Status, :CreatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
                new {
                    Id = ContingencyPlanId,
                    HandoverId = HandoverId,
                    ConditionText = "If BP drops below 90/60",
                    ActionText = "Administer fluids",
                    Priority = "High",
                    Status = "active",
                    CreatedBy = UserId
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Seed Contributors for legacy tests (using numeric IDs from sequence if it exists)
        SeedContributors(connection);
    }

    private void SeedContributors(IDbConnection connection)
    {
        // Ensure CONTRIBUTORS table exists (for legacy tests)
        try {
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
        } catch (OracleException e) when (e.Number == 955) {} // ORA-00955: name already used

        // Ensure CONTRIBUTORS_SEQ sequence exists (without schema prefix for current user)
        try {
            connection.Execute("CREATE SEQUENCE CONTRIBUTORS_SEQ START WITH 1000 INCREMENT BY 1");
        } catch (OracleException e) when (e.Number == 955) {} // ORA-00955: name already used

        // Insert contributors using sequence
        try {
            connection.Execute(@"
                INSERT INTO CONTRIBUTORS (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) 
                VALUES (CONTRIBUTORS_SEQ.NEXTVAL, :Name, 1, NULL, NULL, NULL)", 
                new { Name = TestSeeds.Contributor1 });
        } catch (OracleException e) when (e.Number == 1) {} // Unique constraint

        try {
            connection.Execute(@"
                INSERT INTO CONTRIBUTORS (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) 
                VALUES (CONTRIBUTORS_SEQ.NEXTVAL, :Name, 1, NULL, NULL, NULL)", 
                new { Name = TestSeeds.Contributor2 });
        } catch (OracleException e) when (e.Number == 1) {}
    }
}
