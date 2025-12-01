using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Relevo.FunctionalTests;

/// <summary>
/// Seeds test data for functional tests.
/// 
/// IMPORTANT USAGE GUIDELINES:
/// - IDs de referencia (UnitId, ShiftDayId, ShiftNightId, PatientId1, PatientId2): 
///   Pueden usarse para crear nuevos handovers o como datos de referencia.
/// 
/// - HandoverId seeded: Intended for READ-ONLY tests (GET operations).
///   Tests that MODIFY the handover (POST/PUT/DELETE state changes) should create 
///   their own unique handovers to avoid race conditions and state conflicts.
///   See HandoverLifecycleTests.CreateHandoverWithCoverageForTest() for pattern.
/// 
/// - The seeder runs once when the test host is created, not before each test.
///   This means tests share the same seeded data within a test run.
/// </summary>
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
    public static string HandoverId => $"hvo-{TestRunId}";
    public static string ActionItemId => $"item-{TestRunId}";
    public static string ContingencyPlanId => $"plan-{TestRunId}";
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
        // This is the receiver who will start/complete handovers
        try {
            connection.Execute(@"
                INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES ('dr-1', 'dr-1@example.com', 'Doctor', 'One', 'Dr. One')");
        } catch (OracleException e) when (e.Number == 1) {} // Unique constraint

        // Insert 'dr-sender' user as the sender (different from receiver)
        try {
            connection.Execute(@"
                INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES ('dr-sender', 'dr-sender@example.com', 'Doctor', 'Sender', 'Dr. Sender')");
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

        // V3: Create SHIFT_INSTANCES first, then SHIFT_COVERAGE (replaces USER_ASSIGNMENTS)
        // Create shift instances for today
        var today = DateTime.Today;
        var fromShiftStartAt = today.AddHours(7); // 07:00
        var fromShiftEndAt = today.AddHours(15); // 15:00
        var toShiftStartAt = today.AddHours(19); // 19:00
        var toShiftEndAt = today.AddDays(1).AddHours(7); // 07:00 next day

        string fromShiftInstanceId;
        string toShiftInstanceId;
        
        // Create FROM shift instance
        try {
            fromShiftInstanceId = $"si-from-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_INSTANCES si
                USING (SELECT :Id AS ID FROM DUAL) src ON (si.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT, CREATED_AT, UPDATED_AT)
                    VALUES (:Id, :UnitId, :ShiftId, :StartAt, :EndAt, LOCALTIMESTAMP, LOCALTIMESTAMP)
                WHEN MATCHED THEN
                    UPDATE SET UPDATED_AT = LOCALTIMESTAMP",
                new { Id = fromShiftInstanceId, UnitId = UnitId, ShiftId = ShiftDayId, StartAt = fromShiftStartAt, EndAt = fromShiftEndAt });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Get existing if already created
            fromShiftInstanceId = connection.ExecuteScalar<string>(
                "SELECT ID FROM SHIFT_INSTANCES WHERE UNIT_ID = :UnitId AND SHIFT_ID = :ShiftId AND START_AT = :StartAt",
                new { UnitId = UnitId, ShiftId = ShiftDayId, StartAt = fromShiftStartAt }) ?? $"si-from-{TestRunId}";
        }

        // Create TO shift instance
        try {
            toShiftInstanceId = $"si-to-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_INSTANCES si
                USING (SELECT :Id AS ID FROM DUAL) src ON (si.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT, CREATED_AT, UPDATED_AT)
                    VALUES (:Id, :UnitId, :ShiftId, :StartAt, :EndAt, LOCALTIMESTAMP, LOCALTIMESTAMP)
                WHEN MATCHED THEN
                    UPDATE SET UPDATED_AT = LOCALTIMESTAMP",
                new { Id = toShiftInstanceId, UnitId = UnitId, ShiftId = ShiftNightId, StartAt = toShiftStartAt, EndAt = toShiftEndAt });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Get existing if already created
            toShiftInstanceId = connection.ExecuteScalar<string>(
                "SELECT ID FROM SHIFT_INSTANCES WHERE UNIT_ID = :UnitId AND SHIFT_ID = :ShiftId AND START_AT = :StartAt",
                new { UnitId = UnitId, ShiftId = ShiftNightId, StartAt = toShiftStartAt }) ?? $"si-to-{TestRunId}";
        }

        // V3: Create SHIFT_WINDOW
        string shiftWindowId;
        try {
            shiftWindowId = $"sw-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_WINDOWS sw
                USING (SELECT :Id AS ID FROM DUAL) src ON (sw.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID, CREATED_AT, UPDATED_AT)
                    VALUES (:Id, :UnitId, :FromShiftInstanceId, :ToShiftInstanceId, LOCALTIMESTAMP, LOCALTIMESTAMP)
                WHEN MATCHED THEN
                    UPDATE SET UPDATED_AT = LOCALTIMESTAMP",
                new { Id = shiftWindowId, UnitId = UnitId, FromShiftInstanceId = fromShiftInstanceId, ToShiftInstanceId = toShiftInstanceId });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Get existing if already created
            shiftWindowId = connection.ExecuteScalar<string>(
                "SELECT ID FROM SHIFT_WINDOWS WHERE FROM_SHIFT_INSTANCE_ID = :FromShiftInstanceId AND TO_SHIFT_INSTANCE_ID = :ToShiftInstanceId",
                new { FromShiftInstanceId = fromShiftInstanceId, ToShiftInstanceId = toShiftInstanceId }) ?? $"sw-{TestRunId}";
        }

        // V3: Create SHIFT_COVERAGE (replaces USER_ASSIGNMENTS) for both patients
        try {
            var coverageId1 = $"sc-1-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_COVERAGE sc
                USING (SELECT :Id AS ID FROM DUAL) src ON (sc.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY)
                    VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, LOCALTIMESTAMP, 1)
                WHEN MATCHED THEN
                    UPDATE SET ASSIGNED_AT = LOCALTIMESTAMP",
                new { Id = coverageId1, UserId = "dr-sender", PatientId = PatientId1, ShiftInstanceId = fromShiftInstanceId, UnitId = UnitId });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }

        // Also create coverage for PatientId2
        try {
            var coverageId2 = $"sc-2-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_COVERAGE sc
                USING (SELECT :Id AS ID FROM DUAL) src ON (sc.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY)
                    VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, LOCALTIMESTAMP, 1)
                WHEN MATCHED THEN
                    UPDATE SET ASSIGNED_AT = LOCALTIMESTAMP",
                new { Id = coverageId2, UserId = "dr-sender", PatientId = PatientId2, ShiftInstanceId = fromShiftInstanceId, UnitId = UnitId });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }

        // V3: Create coverage in TO shift for PatientId1 (needed for Start and Complete operations)
        // Use "dr-1" as the receiver (the authenticated client user)
        try {
            var coverageToId1 = $"sc-to-1-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_COVERAGE sc
                USING (SELECT :Id AS ID FROM DUAL) src ON (sc.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY)
                    VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, LOCALTIMESTAMP, 0)
                WHEN MATCHED THEN
                    UPDATE SET ASSIGNED_AT = LOCALTIMESTAMP",
                new { Id = coverageToId1, UserId = "dr-1", PatientId = PatientId1, ShiftInstanceId = toShiftInstanceId, UnitId = UnitId });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }

        // V3: Create coverage in TO shift for PatientId2
        try {
            var coverageToId2 = $"sc-to-2-{TestRunId}";
            connection.Execute(@"
                MERGE INTO SHIFT_COVERAGE sc
                USING (SELECT :Id AS ID FROM DUAL) src ON (sc.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY)
                    VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, LOCALTIMESTAMP, 0)
                WHEN MATCHED THEN
                    UPDATE SET ASSIGNED_AT = LOCALTIMESTAMP",
                new { Id = coverageToId2, UserId = "dr-1", PatientId = PatientId2, ShiftInstanceId = toShiftInstanceId, UnitId = UnitId });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {
            // Unique constraint or foreign key - ignore if already exists or dependency missing
        }

        // V3: Insert test handover using SHIFT_WINDOW_ID
        // IMPORTANT: This handover is intended for READ-ONLY tests (GET operations) and summary tests.
        // Tests that MODIFY the handover (POST/PUT/DELETE state changes) should create their own unique handovers
        // to avoid race conditions and state conflicts.
        // Use MERGE to make it idempotent - will insert if not exists, reset to Draft if exists.
        // Note: UQ_HO_PAT_WINDOW unique constraint prevents multiple active handovers for same patient+window
        // Note: CURRENT_STATE is virtual, calculated from timestamps. Reset all timestamps to ensure Draft state.
        try {
            connection.Execute(@"
                MERGE INTO HANDOVERS h
                USING (SELECT :Id AS ID FROM DUAL) src ON (h.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID, SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID, CREATED_AT, UPDATED_AT)
                    VALUES (:Id, :PatientId, :ShiftWindowId, :UnitId, :SenderUserId, :ReceiverUserId, :CreatedByUserId, LOCALTIMESTAMP, LOCALTIMESTAMP)
                WHEN MATCHED THEN
                    UPDATE SET 
                        UPDATED_AT = LOCALTIMESTAMP,
                        READY_AT = NULL,
                        READY_BY_USER_ID = NULL,
                        STARTED_AT = NULL,
                        STARTED_BY_USER_ID = NULL,
                        COMPLETED_AT = NULL,
                        COMPLETED_BY_USER_ID = NULL,
                        CANCELLED_AT = NULL,
                        CANCELLED_BY_USER_ID = NULL,
                        CANCEL_REASON = NULL",
                new { 
                    Id = HandoverId, 
                    PatientId = PatientId1, 
                    ShiftWindowId = shiftWindowId,
                    UnitId = UnitId,
                    SenderUserId = "dr-sender", // Use dr-sender as sender (different from receiver who will start/complete)
                    ReceiverUserId = "dr-1", // Receiver is dr-1 (the authenticated client)
                    CreatedByUserId = "dr-1"
                });
            
            // Verify handover was created/updated successfully
            var handoverExists = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :Id", 
                new { Id = HandoverId }) > 0;
            
            if (!handoverExists)
            {
                throw new InvalidOperationException($"Failed to create handover {HandoverId} - handover does not exist after MERGE. Check dependencies (shift window, shift instances).");
            }
        } catch (OracleException e) when (e.Number == 1) {
            // Unique constraint (UQ_HO_PAT_WINDOW) - another active handover exists for same patient+window
            // This can happen if a previous test run left an active handover
            // The handover may already exist, which is OK for idempotent seeding
        } catch (OracleException e) when (e.Number == 2291) {
            // Foreign key constraint violation - a dependency doesn't exist
            throw new InvalidOperationException($"Failed to create handover {HandoverId} due to foreign key constraint. Ensure shift window {shiftWindowId} and dependencies exist. Error: {e.Message}", e);
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
                    Priority = "high", // V3: Must be lowercase per CHK_CONT_PRIORITY constraint
                    Status = "active",
                    CreatedBy = UserId
                });
        } catch (OracleException e) when (e.Number == 1 || e.Number == 2291) {}

        // Seed additional data for new endpoints (tables already exist from relevo-api SQL scripts)
        SeedMessages(connection);

        // Seed Contributors for legacy tests
        SeedContributors(connection);
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
