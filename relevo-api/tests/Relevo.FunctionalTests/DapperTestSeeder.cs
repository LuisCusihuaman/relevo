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
    public static string ActivityLogId => $"act-{TestRunId}";
    public static string ChecklistItemId => $"chk-{TestRunId}";
    public static string MessageId => $"msg-{TestRunId}";

    public void Seed()
    {
        string connectionString = configuration.GetConnectionString("OracleConnection")!;
        using var connection = new OracleConnection(connectionString);
        connection.Open();

        SeedTestData(connection);
    }

    private void SeedTestData(IDbConnection connection)
    {
        // Insert hardcoded 'dr-1' user for API endpoints (they use this hardcoded ID)
        try {
            connection.Execute(@"
                INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES ('dr-1', 'dr-1@example.com', 'Doctor', 'One', 'Dr. One')");
        } catch (OracleException e) when (e.Number == 1) {} // Unique constraint

        // Insert test user
        try {
            connection.Execute(@"
                INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES (:Id, :Email, :FirstName, :LastName, :FullName)",
                new { Id = UserId, Email = $"dr-{TestRunId}@example.com", FirstName = "Doctor", LastName = TestRunId, FullName = $"Dr. {TestRunId}" });
        } catch (OracleException e) when (e.Number == 1) {} // Unique constraint

        // Insert FIXED unit for reliable tests (doesn't depend on TestRunId)
        try {
            connection.Execute(@"
                MERGE INTO UNITS u
                USING (SELECT 'test-unit-fixed' AS ID FROM DUAL) src ON (u.ID = src.ID)
                WHEN NOT MATCHED THEN
                INSERT (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT)
                VALUES ('test-unit-fixed', 'Test Unit Fixed', 'Fixed unit for tests', SYSTIMESTAMP, SYSTIMESTAMP)");
        } catch (OracleException) {}

        // Insert test unit (dynamic)
        try {
            connection.Execute(@"
                INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = UnitId, Name = $"UCI-{TestRunId}", Description = "Test Unit" });
        } catch (OracleException e) when (e.Number == 1) {}

        // Insert FIXED shifts for reliable tests
        try {
            connection.Execute(@"
                MERGE INTO SHIFTS s
                USING (SELECT 'shift-day-fixed' AS ID FROM DUAL) src ON (s.ID = src.ID)
                WHEN NOT MATCHED THEN
                INSERT (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT)
                VALUES ('shift-day-fixed', 'Day Fixed', '07:00', '15:00', SYSTIMESTAMP, SYSTIMESTAMP)");
        } catch (OracleException) {}

        try {
            connection.Execute(@"
                MERGE INTO SHIFTS s
                USING (SELECT 'shift-night-fixed' AS ID FROM DUAL) src ON (s.ID = src.ID)
                WHEN NOT MATCHED THEN
                INSERT (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT)
                VALUES ('shift-night-fixed', 'Night Fixed', '19:00', '07:00', SYSTIMESTAMP, SYSTIMESTAMP)");
        } catch (OracleException) {}

        // Insert test shifts (dynamic)
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

        // Insert FIXED patient for reliable tests
        try {
            connection.Execute(@"
                MERGE INTO PATIENTS p
                USING (SELECT 'patient-fixed' AS ID FROM DUAL) src ON (p.ID = src.ID)
                WHEN NOT MATCHED THEN
                INSERT (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
                VALUES ('patient-fixed', 'Test Patient Fixed', 'test-unit-fixed', DATE '2010-01-01', 'Male', SYSTIMESTAMP, '100', 'Test Diagnosis Fixed', SYSTIMESTAMP, SYSTIMESTAMP)");
        } catch (OracleException) {}

        // Insert test patients (dynamic)
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

        // Insert test assignment - use MERGE to make it idempotent
        try {
            connection.Execute(@"
                MERGE INTO USER_ASSIGNMENTS ua
                USING (SELECT :Id AS ASSIGNMENT_ID FROM DUAL) src ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
                WHEN NOT MATCHED THEN
                    INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
                    VALUES (:Id, :UserId, :ShiftId, :PatientId, LOCALTIMESTAMP)
                WHEN MATCHED THEN
                    UPDATE SET ASSIGNED_AT = LOCALTIMESTAMP",
                new { Id = AssignmentId, UserId = UserId, ShiftId = ShiftDayId, PatientId = PatientId1 });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }

        // Insert test handover (new schema: no STATUS, ASSIGNMENT_ID, SHIFT_NAME, CREATED_BY, RESPONSIBLE_PHYSICIAN_ID, HANDOVER_TYPE)
        // Use MERGE to make it idempotent - will update if exists, insert if not
        // When updating, reset state to Draft by clearing all state timestamps
        // Note: UQ_HO_ACTIVE_WINDOW unique constraint prevents multiple active handovers for same patient/window/shift
        try {
            var rowsAffected = connection.Execute(@"
                MERGE INTO HANDOVERS h
                USING (SELECT :Id AS ID FROM DUAL) src ON (h.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT, CREATED_AT, UPDATED_AT)
                    VALUES (:Id, :PatientId, :FromShiftId, :ToShiftId, :FromUserId, :ToUserId, LOCALTIMESTAMP, LOCALTIMESTAMP, LOCALTIMESTAMP)
                WHEN MATCHED THEN
                    UPDATE SET 
                        UPDATED_AT = LOCALTIMESTAMP,
                        READY_AT = NULL,
                        STARTED_AT = NULL,
                        ACCEPTED_AT = NULL,
                        COMPLETED_AT = NULL,
                        CANCELLED_AT = NULL,
                        REJECTED_AT = NULL,
                        EXPIRED_AT = NULL,
                        REJECTION_REASON = NULL",
                new { 
                    Id = HandoverId, 
                    PatientId = PatientId1, 
                    FromShiftId = ShiftDayId,
                    ToShiftId = ShiftNightId,
                    FromUserId = UserId,
                    ToUserId = UserId
                });
            
            // Verify handover was created/updated successfully
            var handoverExists = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :Id", 
                new { Id = HandoverId }) > 0;
            
            if (!handoverExists)
            {
                throw new InvalidOperationException($"Failed to create handover {HandoverId} - handover does not exist after MERGE");
            }
        } catch (OracleException e) when (e.Number == 1) {
            // Unique constraint violation (UQ_HO_ACTIVE_WINDOW) - another active handover exists for same patient/window/shift
            // This can happen if a previous test run left an active handover. Cancel the existing one and retry.
            var existingHandoverId = connection.ExecuteScalar<string>(
                @"SELECT ID FROM HANDOVERS 
                  WHERE PATIENT_ID = :PatientId 
                    AND FROM_SHIFT_ID = :FromShiftId 
                    AND TO_SHIFT_ID = :ToShiftId
                    AND COMPLETED_AT IS NULL
                    AND CANCELLED_AT IS NULL
                    AND REJECTED_AT IS NULL
                    AND EXPIRED_AT IS NULL
                    AND ROWNUM = 1",
                new { PatientId = PatientId1, FromShiftId = ShiftDayId, ToShiftId = ShiftNightId });
            
            if (existingHandoverId != null)
            {
                // Cancel the existing handover to make it inactive (no longer violates unique constraint)
                connection.Execute(@"
                    UPDATE HANDOVERS SET 
                        CANCELLED_AT = LOCALTIMESTAMP,
                        UPDATED_AT = LOCALTIMESTAMP
                    WHERE ID = :Id",
                    new { Id = existingHandoverId });
                
                // Now retry the MERGE
                connection.Execute(@"
                    MERGE INTO HANDOVERS h
                    USING (SELECT :Id AS ID FROM DUAL) src ON (h.ID = src.ID)
                    WHEN NOT MATCHED THEN
                        INSERT (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT, CREATED_AT, UPDATED_AT)
                        VALUES (:Id, :PatientId, :FromShiftId, :ToShiftId, :FromUserId, :ToUserId, LOCALTIMESTAMP, LOCALTIMESTAMP, LOCALTIMESTAMP)
                    WHEN MATCHED THEN
                        UPDATE SET 
                            UPDATED_AT = LOCALTIMESTAMP,
                            READY_AT = NULL,
                            STARTED_AT = NULL,
                            ACCEPTED_AT = NULL,
                            COMPLETED_AT = NULL,
                            CANCELLED_AT = NULL,
                            REJECTED_AT = NULL,
                            EXPIRED_AT = NULL,
                            REJECTION_REASON = NULL",
                    new { 
                        Id = HandoverId, 
                        PatientId = PatientId1, 
                        FromShiftId = ShiftDayId,
                        ToShiftId = ShiftNightId,
                        FromUserId = UserId,
                        ToUserId = UserId
                    });
            }
            // If we can't find an existing handover, the constraint violation is unexpected - let it propagate
        } catch (OracleException e) when (e.Number == 2291) {
            // Foreign key constraint violation - log but don't fail
            // This means a dependency doesn't exist, which shouldn't happen if seeding order is correct
            throw new InvalidOperationException($"Failed to create handover {HandoverId} due to foreign key constraint: {e.Message}", e);
        }

        // Insert test handover contents (merged table replaces HANDOVER_PATIENT_DATA, HANDOVER_SYNTHESIS, HANDOVER_SITUATION_AWARENESS)
        // Note: HANDOVER_CONTENTS doesn't have CREATED_AT, only UPDATED_AT
        // Use MERGE to make it idempotent and handle race conditions
        try {
            connection.Execute(@"
                MERGE INTO HANDOVER_CONTENTS hc
                USING (SELECT :HandoverId AS HANDOVER_ID FROM DUAL) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
                WHEN NOT MATCHED THEN
                    INSERT (
                        HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, SYNTHESIS,
                        PATIENT_SUMMARY_STATUS, SA_STATUS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT
                    ) VALUES (
                        :HandoverId, :Severity, :Summary, :SituationAwareness, '',
                        'Draft', 'Draft', 'Draft', :LastEditedBy, LOCALTIMESTAMP
                    )
                WHEN MATCHED THEN
                    UPDATE SET UPDATED_AT = LOCALTIMESTAMP",
                new { 
                    HandoverId = HandoverId, 
                    Severity = "Stable", 
                    Summary = "Patient stable overnight",
                    SituationAwareness = "Initial SA",
                    LastEditedBy = UserId
                });
        } catch (OracleException e) when (e.Number == 2291 || e.Number == 1) {
            // Foreign key constraint violation (handover doesn't exist) or unique constraint
            // This can happen in race conditions - ignore and continue
        }

        // Insert test action item
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :HandoverId, :Description, :IsCompleted, LOCALTIMESTAMP, LOCALTIMESTAMP)",
                new { 
                    Id = ActionItemId, 
                    HandoverId = HandoverId, 
                    Description = "Check blood pressure",
                    IsCompleted = 0
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // PATIENT_SUMMARIES table removed in new schema
        // Patient summary is now stored in HANDOVER_CONTENTS.PATIENT_SUMMARY per handover
        // Skipping PATIENT_SUMMARIES insert

        // Insert test contingency plan
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :HandoverId, :ConditionText, :ActionText, :Priority, :Status, :CreatedBy, LOCALTIMESTAMP, LOCALTIMESTAMP)",
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

        // Seed additional data for new endpoints (tables already exist from relevo-api SQL scripts)
        SeedActivityLogs(connection);
        SeedChecklists(connection);
        SeedMessages(connection);

        // Seed Contributors for legacy tests
        SeedContributors(connection);
    }

    private void SeedActivityLogs(IDbConnection connection)
    {
        // Insert test activity log (schema updated: ACTIVITY_DESCRIPTION/SECTION_AFFECTED -> DESCRIPTION, FROM_STATE, TO_STATE, REASON)
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_ACTIVITY_LOG (ID, HANDOVER_ID, USER_ID, ACTIVITY_TYPE, DESCRIPTION, CREATED_AT)
                VALUES (:Id, :HandoverId, :UserId, :ActivityType, :Description, LOCALTIMESTAMP)",
                new {
                    Id = ActivityLogId,
                    HandoverId = HandoverId,
                    UserId = UserId,
                    ActivityType = "StateChange",
                    Description = "Handover created"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291 || e.Number == 942) {} // 942 = table not exists
    }

    private void SeedChecklists(IDbConnection connection)
    {
        // HANDOVER_CHECKLISTS table removed in new schema
        // Skipping checklist insert
    }

    private void SeedMessages(IDbConnection connection)
    {
        // Insert test message (USER_NAME removed from HANDOVER_MESSAGES in new schema)
        try {
            connection.Execute(@"
                INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :HandoverId, :UserId, :MessageText, :MessageType, LOCALTIMESTAMP, LOCALTIMESTAMP)",
                new {
                    Id = MessageId,
                    HandoverId = HandoverId,
                    UserId = UserId,
                    MessageText = "Initial handover message",
                    MessageType = "message"
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291 || e.Number == 942) {}
    }

    private void SeedContributors(IDbConnection connection)
    {
        // CONTRIBUTORS table/sequence are NOT in relevo-api SQL scripts - they're only in the new api project
        // So we need to ensure they exist here
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

        try {
            connection.Execute("CREATE SEQUENCE CONTRIBUTORS_SEQ START WITH 1000 INCREMENT BY 1");
        } catch (OracleException e) when (e.Number == 955) {} // ORA-00955: name already used

        // Use MERGE to make contributor inserts idempotent (prevents race conditions in parallel tests)
        // Insert contributor with ID=1 for test that expects GetContributorById(1)
        try {
            connection.Execute(@"
                MERGE INTO CONTRIBUTORS c
                USING (SELECT 1 AS Id FROM DUAL) src ON (c.Id = src.Id)
                WHEN NOT MATCHED THEN
                    INSERT (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) 
                    VALUES (1, :Name, 1, NULL, NULL, NULL)
                WHEN MATCHED THEN
                    UPDATE SET Name = :Name", 
                new { Name = TestSeeds.Contributor1 });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }

        // Insert contributor with ID=2 using MERGE
        try {
            connection.Execute(@"
                MERGE INTO CONTRIBUTORS c
                USING (SELECT 2 AS Id FROM DUAL) src ON (c.Id = src.Id)
                WHEN NOT MATCHED THEN
                    INSERT (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension) 
                    VALUES (2, :Name, 1, NULL, NULL, NULL)
                WHEN MATCHED THEN
                    UPDATE SET Name = :Name", 
                new { Name = TestSeeds.Contributor2 });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }
    }
}
