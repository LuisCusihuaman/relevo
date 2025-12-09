using Dapper;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Handover state machine operations - Create, Ready, Start, Complete, Cancel, Reject.
/// State flow: Draft -> Ready -> InProgress -> Completed/Cancelled
/// </summary>
public partial class HandoverRepository
{
    public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
    {
        using var conn = _connectionFactory.CreateConnection();

        // V3: Get patient's UNIT_ID
        var unitId = await conn.ExecuteScalarAsync<string>(
            "SELECT UNIT_ID FROM PATIENTS WHERE ID = :patientId",
            new { patientId = request.PatientId });

        if (string.IsNullOrEmpty(unitId))
        {
            throw new InvalidOperationException($"Patient {request.PatientId} not found or has no unit");
        }

        // V3: Calculate shift instance dates using shared service
        var today = DateTime.Today;
        var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
            conn, request.FromShiftId, request.ToShiftId, today);

        if (shiftDates == null)
        {
            throw new InvalidOperationException($"Shift templates not found: FromShiftId={request.FromShiftId}, ToShiftId={request.ToShiftId}");
        }

        var (fromShiftStartAt, fromShiftEndAt, toShiftStartAt, toShiftEndAt) = shiftDates.Value;

        // V3: Get or create shift instances
        var fromShiftInstanceId = await ShiftInstanceRepository.GetOrCreateShiftInstanceAsync(
            request.FromShiftId, unitId, fromShiftStartAt, fromShiftEndAt);
        
        var toShiftInstanceId = await ShiftInstanceRepository.GetOrCreateShiftInstanceAsync(
            request.ToShiftId, unitId, toShiftStartAt, toShiftEndAt);

        // V3: Get or create shift window
        var shiftWindowId = await ShiftWindowRepository.GetOrCreateShiftWindowAsync(
            fromShiftInstanceId, toShiftInstanceId, unitId);

        // V3: Get SENDER_USER_ID from SHIFT_COVERAGE (primary responsible for FROM shift)
        var senderUserId = await conn.ExecuteScalarAsync<string>(
            @"SELECT RESPONSIBLE_USER_ID
              FROM (
                SELECT RESPONSIBLE_USER_ID
                FROM SHIFT_COVERAGE
                WHERE PATIENT_ID = :patientId
                  AND SHIFT_INSTANCE_ID = :shiftInstanceId
                  AND IS_PRIMARY = 1
                ORDER BY ASSIGNED_AT ASC
              ) WHERE ROWNUM <= 1",
            new { patientId = request.PatientId, shiftInstanceId = fromShiftInstanceId });

        // If no primary, get the first one
        if (string.IsNullOrEmpty(senderUserId))
        {
            senderUserId = await conn.ExecuteScalarAsync<string>(
                @"SELECT RESPONSIBLE_USER_ID
                  FROM (
                    SELECT RESPONSIBLE_USER_ID
                    FROM SHIFT_COVERAGE
                    WHERE PATIENT_ID = :patientId
                      AND SHIFT_INSTANCE_ID = :shiftInstanceId
                    ORDER BY ASSIGNED_AT ASC
                  ) WHERE ROWNUM <= 1",
                new { patientId = request.PatientId, shiftInstanceId = fromShiftInstanceId });
        }

        // V3_PLAN.md regla #10: Cannot create handover without coverage
        if (string.IsNullOrEmpty(senderUserId))
        {
            throw new InvalidOperationException(
                $"Cannot create handover: patient {request.PatientId} has no coverage in FROM shift instance {fromShiftInstanceId}. " +
                "A handover cannot exist without coverage.");
        }

        // V3: Find previous handover for the same patient (most recent completed handover)
        var previousHandoverId = await conn.ExecuteScalarAsync<string>(@"
            SELECT ID FROM (
                SELECT ID
                FROM HANDOVERS
                WHERE PATIENT_ID = :patientId
                  AND COMPLETED_AT IS NOT NULL
                  AND CANCELLED_AT IS NULL
                ORDER BY COMPLETED_AT DESC
            ) WHERE ROWNUM <= 1",
            new { patientId = request.PatientId });

        // V3_PLAN.md Regla #36: Copy PATIENT_SUMMARY from previous handover
        string? previousPatientSummary = null;
        if (!string.IsNullOrEmpty(previousHandoverId))
        {
            previousPatientSummary = await conn.ExecuteScalarAsync<string>(@"
                SELECT PATIENT_SUMMARY
                FROM HANDOVER_CONTENTS
                WHERE HANDOVER_ID = :previousHandoverId",
                new { previousHandoverId });
        }

        // V3_PLAN.md Regla #16: Idempotency via DB constraint UQ_HO_PAT_WINDOW
        // V3_PLAN.md Regla #9: máximo 1 handover activo por paciente por ventana
        var handoverId = Guid.NewGuid().ToString();
        const string mergeSql = @"
            MERGE INTO HANDOVERS h
            USING (
                SELECT :patientId AS PATIENT_ID, :shiftWindowId AS SHIFT_WINDOW_ID FROM DUAL
            ) src ON (
                h.PATIENT_ID = src.PATIENT_ID AND h.SHIFT_WINDOW_ID = src.SHIFT_WINDOW_ID
            )
            WHEN NOT MATCHED THEN
                INSERT (
                    ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID,
                    PREVIOUS_HANDOVER_ID, SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID,
                    CREATED_AT, UPDATED_AT
                ) VALUES (
                    :id, :patientId, :shiftWindowId, :unitId,
                    :previousHandoverId, :senderUserId, :receiverUserId, :createdByUserId,
                    LOCALTIMESTAMP, LOCALTIMESTAMP
                )";

        await conn.ExecuteAsync(mergeSql, new
        {
            id = handoverId,
            patientId = request.PatientId,
            shiftWindowId = shiftWindowId,
            unitId = unitId,
            previousHandoverId = previousHandoverId,
            senderUserId = senderUserId,
            receiverUserId = request.ToDoctorId,
            createdByUserId = request.InitiatedBy
        });

        // Get the actual handover ID (might be existing one if MERGE matched)
        const string getHandoverIdSql = @"
            SELECT ID FROM HANDOVERS
            WHERE PATIENT_ID = :patientId AND SHIFT_WINDOW_ID = :shiftWindowId
              AND ROWNUM <= 1";
        
        var actualHandoverId = await conn.ExecuteScalarAsync<string>(getHandoverIdSql, new
        {
            patientId = request.PatientId,
            shiftWindowId = shiftWindowId
        });

        if (string.IsNullOrEmpty(actualHandoverId))
        {
            throw new InvalidOperationException($"Handover not found after MERGE for PatientId={request.PatientId}, ShiftWindowId={shiftWindowId}");
        }

        // Only create HANDOVER_CONTENTS if this is a new handover (not existing one)
        if (actualHandoverId == handoverId)
        {
            // V3_PLAN.md Regla #36: Copy PATIENT_SUMMARY from previous handover if available
            await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_CONTENTS (
                    HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, SYNTHESIS,
                    PATIENT_SUMMARY_STATUS, SA_STATUS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT
                ) VALUES (
                    :handoverId, 'Stable', :patientSummary, NULL, NULL,
                    'Draft', 'Draft', 'Draft', :userId, LOCALTIMESTAMP
                )",
                new { handoverId = actualHandoverId, patientSummary = previousPatientSummary, userId = request.InitiatedBy });
        }

        // Fetch and return the handover (existing or newly created)
        var handoverDetail = await GetHandoverByIdAsync(actualHandoverId);
        if (handoverDetail == null)
        {
            throw new InvalidOperationException($"Failed to retrieve handover {actualHandoverId}");
        }

        return handoverDetail.Handover;
    }

    public async Task<bool> MarkAsReadyAsync(string handoverId, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            const string getHandoverSql = @"
                SELECT SHIFT_WINDOW_ID, PATIENT_ID, SENDER_USER_ID
                FROM HANDOVERS
                WHERE ID = :handoverId";
            
            var handover = await conn.QueryFirstOrDefaultAsync<dynamic>(getHandoverSql, new { handoverId });
            
            if (handover == null)
            {
                return false;
            }

            string? shiftWindowId = handover.SHIFT_WINDOW_ID;
            string patientId = handover.PATIENT_ID;
            string? existingSenderUserId = handover.SENDER_USER_ID;

            // V3_PLAN.md regla #10: Validate coverage >= 1 before Ready
            string? fromShiftInstanceId = null;
            if (!string.IsNullOrEmpty(shiftWindowId))
            {
                const string getWindowSql = @"
                    SELECT FROM_SHIFT_INSTANCE_ID, UNIT_ID
                    FROM SHIFT_WINDOWS
                    WHERE ID = :shiftWindowId";
                
                var window = await conn.QueryFirstOrDefaultAsync<dynamic>(getWindowSql, new { shiftWindowId });
                if (window != null)
                {
                    fromShiftInstanceId = window.FROM_SHIFT_INSTANCE_ID;
                    
                    var coverageCount = await conn.ExecuteScalarAsync<int>(@"
                        SELECT COUNT(*)
                        FROM SHIFT_COVERAGE
                        WHERE PATIENT_ID = :patientId
                          AND SHIFT_INSTANCE_ID = :fromShiftInstanceId",
                        new { patientId, fromShiftInstanceId });

                    if (coverageCount == 0)
                    {
                        return false;
                    }
                }
            }

            // If SENDER_USER_ID is not set, get it from SHIFT_COVERAGE
            string? senderUserId = existingSenderUserId;
            if (string.IsNullOrEmpty(senderUserId) && !string.IsNullOrEmpty(fromShiftInstanceId))
            {
                const string getPrimarySql = @"
                    SELECT RESPONSIBLE_USER_ID
                    FROM SHIFT_COVERAGE
                    WHERE PATIENT_ID = :patientId
                      AND SHIFT_INSTANCE_ID = :shiftInstanceId
                      AND IS_PRIMARY = 1
                      AND ROWNUM <= 1";
                
                senderUserId = await conn.ExecuteScalarAsync<string>(getPrimarySql, new { patientId, shiftInstanceId = fromShiftInstanceId });
                
                if (string.IsNullOrEmpty(senderUserId))
                {
                    const string getFirstSql = @"
                        SELECT RESPONSIBLE_USER_ID
                        FROM (
                            SELECT RESPONSIBLE_USER_ID
                            FROM SHIFT_COVERAGE
                            WHERE PATIENT_ID = :patientId
                              AND SHIFT_INSTANCE_ID = :shiftInstanceId
                            ORDER BY ASSIGNED_AT ASC
                        ) WHERE ROWNUM <= 1";
                    
                    senderUserId = await conn.ExecuteScalarAsync<string>(getFirstSql, new { patientId, shiftInstanceId = fromShiftInstanceId });
                }
            }

            const string sql = @"
                UPDATE HANDOVERS
                SET READY_AT = SYSTIMESTAMP,
                    READY_BY_USER_ID = :userId,
                    SENDER_USER_ID = COALESCE(SENDER_USER_ID, :senderUserId),
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId
                  AND READY_AT IS NULL";

            var rows = await conn.ExecuteAsync(sql, new { handoverId, userId, senderUserId });
            
            // Idempotent: if already Ready, return true
            if (rows == 0)
            {
                var alreadyReady = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId AND READY_AT IS NOT NULL",
                    new { handoverId }) > 0;
                return alreadyReady;
            }
            return rows > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ReturnForChangesAsync(string handoverId, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            // V3_PLAN.md regla #21: ReturnForChanges vuelve a Draft limpiando READY_AT
            const string sql = @"
                UPDATE HANDOVERS
                SET READY_AT = NULL,
                    READY_BY_USER_ID = NULL,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId
                  AND READY_AT IS NOT NULL
                  AND COMPLETED_AT IS NULL
                  AND CANCELLED_AT IS NULL";
            
            var rows = await conn.ExecuteAsync(sql, new { handoverId });
            return rows > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> StartHandoverAsync(string handoverId, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            // V3: Set STARTED_AT and STARTED_BY_USER_ID
            // Constraint CHK_HO_STARTED_NE_SENDER ensures STARTED_BY_USER_ID <> SENDER_USER_ID
            const string sql = @"
                UPDATE HANDOVERS
                SET STARTED_AT = SYSTIMESTAMP,
                    STARTED_BY_USER_ID = :userId,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId
                  AND STARTED_AT IS NULL
                  AND READY_AT IS NOT NULL";

            var rows = await conn.ExecuteAsync(sql, new { handoverId, userId });
            return rows > 0;
        }
        catch (OracleException ex) when (ex.Number == 2290)
        {
            // ORA-02290: check constraint violated
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartHandoverAsync exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RejectHandoverAsync(string handoverId, string cancelReason, string userId)
    {
        // V3: Reject uses Cancel with CANCEL_REASON='ReceiverRefused'
        return await CancelHandoverAsync(handoverId, cancelReason, userId);
    }

    public async Task<bool> CancelHandoverAsync(string handoverId, string cancelReason, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            // V3: Set CANCELLED_AT, CANCELLED_BY_USER_ID, and CANCEL_REASON
            const string sql = @"
                UPDATE HANDOVERS
                SET CANCELLED_AT = SYSTIMESTAMP,
                    CANCELLED_BY_USER_ID = :userId,
                    CANCEL_REASON = :cancelReason,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId
                  AND CANCELLED_AT IS NULL
                  AND COMPLETED_AT IS NULL";

            var rows = await conn.ExecuteAsync(sql, new { handoverId, userId, cancelReason });
            return rows > 0;
        }
        catch (OracleException ex) when (ex.Number == 2290)
        {
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CancelHandoverAsync exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            // V3: Set COMPLETED_AT and COMPLETED_BY_USER_ID
            // Constraint CHK_HO_COMPLETED_NE_SENDER ensures COMPLETED_BY_USER_ID <> SENDER_USER_ID
            const string sql = @"
                UPDATE HANDOVERS
                SET COMPLETED_AT = SYSTIMESTAMP,
                    COMPLETED_BY_USER_ID = :userId,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId
                  AND COMPLETED_AT IS NULL
                  AND CANCELLED_AT IS NULL
                  AND STARTED_AT IS NOT NULL";

            var rows = await conn.ExecuteAsync(sql, new { handoverId, userId });
            return rows > 0;
        }
        catch (OracleException ex) when (ex.Number == 2290)
        {
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CompleteHandoverAsync exception: {ex.Message}");
            return false;
        }
    }
}
