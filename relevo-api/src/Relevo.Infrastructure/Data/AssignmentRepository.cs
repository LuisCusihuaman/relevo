using System.Data;
using Dapper;
using MediatR;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Events;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class AssignmentRepository(
    DapperConnectionFactory _connectionFactory,
    IShiftInstanceRepository _shiftInstanceRepository,
    IMediator _mediator) : IAssignmentRepository
{
    /// <summary>
    /// Helper method to remove coverage with automatic primary promotion.
    /// If the removed user was primary, promotes the next oldest coverage to primary.
    /// </summary>
    private async Task<bool> RemoveCoverageWithPrimaryPromotionAsync(
        IDbConnection conn,
        string userId,
        string shiftInstanceId,
        string patientId)
    {
        // 1. Check if the user being removed is primary
        var deletedIsPrimary = await conn.ExecuteScalarAsync<int>(@"
            SELECT IS_PRIMARY FROM SHIFT_COVERAGE
            WHERE RESPONSIBLE_USER_ID = :userId
              AND SHIFT_INSTANCE_ID = :shiftInstanceId
              AND PATIENT_ID = :patientId",
            new { userId, shiftInstanceId, patientId });

        // 2. Delete coverage
        var deleted = await conn.ExecuteAsync(@"
            DELETE FROM SHIFT_COVERAGE
            WHERE RESPONSIBLE_USER_ID = :userId
              AND SHIFT_INSTANCE_ID = :shiftInstanceId
              AND PATIENT_ID = :patientId",
            new { userId, shiftInstanceId, patientId });

        if (deleted == 0) return false;

        // 3. If was primary, promote the next oldest coverage to primary
        if (deletedIsPrimary == 1)
        {
            await conn.ExecuteAsync(@"
                UPDATE SHIFT_COVERAGE
                SET IS_PRIMARY = 1
                WHERE PATIENT_ID = :patientId
                  AND SHIFT_INSTANCE_ID = :shiftInstanceId
                  AND RESPONSIBLE_USER_ID = (
                    SELECT RESPONSIBLE_USER_ID FROM (
                      SELECT RESPONSIBLE_USER_ID
                      FROM SHIFT_COVERAGE
                      WHERE PATIENT_ID = :patientId
                        AND SHIFT_INSTANCE_ID = :shiftInstanceId
                      ORDER BY ASSIGNED_AT ASC
                    ) WHERE ROWNUM <= 1
                  )",
                new { patientId, shiftInstanceId });
        }

        return true;
    }
    public async Task<IReadOnlyList<string>> AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();

            var patientIdList = patientIds.ToList();
            if (patientIdList.Count == 0)
                return patientIdList;

            // Get shift template to determine start/end times
            const string getShiftSql = @"
                SELECT START_TIME, END_TIME
                FROM SHIFTS
                WHERE ID = :shiftId";
            
            var shift = await conn.QueryFirstOrDefaultAsync<dynamic>(getShiftSql, new { shiftId });
            if (shift == null)
            {
                throw new InvalidOperationException($"Shift template not found: {shiftId}");
            }

            string startTime = shift.START_TIME; // Format: HH:MM
            string endTime = shift.END_TIME; // Format: HH:MM

            // Parse times and create shift instance for today
            var today = DateTime.Today;
            var startTimeParts = startTime.Split(':');
            var endTimeParts = endTime.Split(':');
            
            var startAt = today.AddHours(int.Parse(startTimeParts[0])).AddMinutes(int.Parse(startTimeParts[1]));
            var endAt = today.AddHours(int.Parse(endTimeParts[0])).AddMinutes(int.Parse(endTimeParts[1]));
            
            // Handle overnight shifts (end time < start time means it goes to next day)
            if (endAt < startAt)
            {
                endAt = endAt.AddDays(1);
            }

            // Process each patient
            var assignedPatientIds = new List<string>();
            
            foreach (var patientId in patientIdList)
            {
                // Get patient's unit
                var unitId = await conn.ExecuteScalarAsync<string>(
                    "SELECT UNIT_ID FROM PATIENTS WHERE ID = :patientId",
                    new { patientId });
                
                if (string.IsNullOrEmpty(unitId))
                {
                    continue; // Skip if patient not found
                }

                // Get or create shift instance for this shift, unit, and date
                var shiftInstanceId = await _shiftInstanceRepository.GetOrCreateShiftInstanceAsync(
                    shiftId, unitId, startAt, endAt);

                // Get shift instance details for event (needed for dates)
                var shiftInstance = await _shiftInstanceRepository.GetShiftInstanceByIdAsync(shiftInstanceId);
                if (shiftInstance == null)
                {
                    throw new InvalidOperationException($"Shift instance {shiftInstanceId} not found after creation");
                }

                // Remove existing coverage for this user, patient, and shift instance (with primary promotion)
                await RemoveCoverageWithPrimaryPromotionAsync(conn, userId, shiftInstanceId, patientId);

                // Check if there's already a primary for this patient+shift_instance
                var existingPrimary = await conn.ExecuteScalarAsync<string>(@"
                    SELECT RESPONSIBLE_USER_ID
                    FROM SHIFT_COVERAGE
                    WHERE PATIENT_ID = :patientId
                      AND SHIFT_INSTANCE_ID = :shiftInstanceId
                      AND IS_PRIMARY = 1
                      AND ROWNUM <= 1",
                    new { patientId, shiftInstanceId });

                // Set IS_PRIMARY=1 if no primary exists for this patient+shift_instance
                var isPrimary = string.IsNullOrEmpty(existingPrimary) ? 1 : 0;

                // Insert coverage
                var coverageId = $"sc-{Guid.NewGuid().ToString()[..8]}";
                await conn.ExecuteAsync(@"
                    INSERT INTO SHIFT_COVERAGE (
                        ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY
                    ) VALUES (
                        :coverageId, :userId, :patientId, :shiftInstanceId, :unitId, LOCALTIMESTAMP, :isPrimary
                    )",
                    new { coverageId, userId, patientId, shiftInstanceId, unitId, isPrimary });

                // V3_PLAN.md Regla #14: Publish domain event to trigger automatic handover creation
                // Only publish if this is a primary assignment (handover creation requires primary sender)
                if (isPrimary == 1)
                {
                    var domainEvent = new PatientAssignedToShiftEvent(
                        patientId: patientId,
                        userId: userId,
                        shiftId: shiftId,
                        shiftInstanceId: shiftInstanceId,
                        unitId: unitId,
                        shiftStartAt: shiftInstance.StartAt,
                        shiftEndAt: shiftInstance.EndAt,
                        isPrimary: true
                    );
                    
                    // Publish event asynchronously (fire-and-forget to avoid blocking assignment)
                    // If handover creation fails, it can be created manually later
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _mediator.Publish(domainEvent);
                        }
                        catch (Exception ex)
                        {
                            // Log but don't fail assignment if event publishing fails
                            Console.WriteLine($"Error publishing PatientAssignedToShiftEvent for PatientId={patientId}, ShiftId={shiftId}: {ex.Message}");
                        }
                    }, cancellationToken: default);
                }

                assignedPatientIds.Add(patientId);
            }

            return assignedPatientIds;
        }
        catch (Oracle.ManagedDataAccess.Client.OracleException ex)
        {
            Console.WriteLine($"Error in AssignPatientsAsync: {ex.Number} - {ex.Message}");
            throw;
        }
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Get total count - patients where user has coverage in current or recent shift instances
        // For simplicity, get patients from shift instances that started today or later
        const string countSql = @"
            SELECT COUNT(DISTINCT p.ID) 
            FROM PATIENTS p 
            INNER JOIN SHIFT_COVERAGE sc ON p.ID = sc.PATIENT_ID
            INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
            WHERE sc.RESPONSIBLE_USER_ID = :userId
              AND si.START_AT >= TRUNC(SYSDATE)";

        var total = await conn.ExecuteScalarAsync<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<PatientRecord>(), 0);

        var p_ = Math.Max(page, 1);
        var ps = Math.Max(pageSize, 1);
        var offset = (p_ - 1) * ps;

        // Get patients with pagination
        const string pageSql = @"
            SELECT * FROM (
                SELECT 
                    p.ID AS Id, 
                    p.NAME AS Name, 
                    'not-started' AS HandoverStatus,
                    CAST(NULL AS VARCHAR2(255)) AS HandoverId,
                    FLOOR(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH) / 12) AS Age,
                    p.ROOM_NUMBER AS Room,
                    p.DIAGNOSIS AS Diagnosis,
                    CAST(NULL AS VARCHAR2(50)) AS Status,
                    CAST(NULL AS VARCHAR2(50)) AS Severity,
                    ROW_NUMBER() OVER (ORDER BY p.NAME) AS rn
                FROM PATIENTS p
                INNER JOIN SHIFT_COVERAGE sc ON p.ID = sc.PATIENT_ID
                INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
                WHERE sc.RESPONSIBLE_USER_ID = :userId
                  AND si.START_AT >= TRUNC(SYSDATE)
            ) WHERE rn > :offset AND rn <= :maxRow";

        var items = await conn.QueryAsync<PatientRecord>(pageSql, new { userId, offset, maxRow = offset + ps });
        return (items.ToList(), total);
    }

    public async Task<bool> UnassignPatientAsync(string userId, string shiftInstanceId, string patientId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            return await RemoveCoverageWithPrimaryPromotionAsync(conn, userId, shiftInstanceId, patientId);
        }
        catch (Oracle.ManagedDataAccess.Client.OracleException ex)
        {
            Console.WriteLine($"Error in UnassignPatientAsync: {ex.Number} - {ex.Message}");
            throw;
        }
    }
}

