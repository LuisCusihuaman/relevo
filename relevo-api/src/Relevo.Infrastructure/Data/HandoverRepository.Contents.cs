using Dapper;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Handover contents operations - Clinical data, Synthesis, Situation Awareness.
/// Manages the HANDOVER_CONTENTS table.
/// </summary>
public partial class HandoverRepository
{
    #region Synthesis

    public async Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT HANDOVER_ID as HandoverId, SYNTHESIS as Content, SYNTHESIS_STATUS as STATUS,
                   LAST_EDITED_BY as LastEditedBy, UPDATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM HANDOVER_CONTENTS
            WHERE HANDOVER_ID = :handoverId";

        var result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });

        if (result == null)
        {
            var createdBy = await conn.ExecuteScalarAsync<string>(
                "SELECT CREATED_BY_USER_ID FROM HANDOVERS WHERE ID = :handoverId",
                new { handoverId });

            if (createdBy == null) return null;

            await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_CONTENTS (
                    HANDOVER_ID, SYNTHESIS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT,
                    ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, PATIENT_SUMMARY_STATUS, SA_STATUS
                ) VALUES (
                    :handoverId, '', 'Draft', :createdBy, LOCALTIMESTAMP,
                    'Stable', '', '', 'Draft', 'Draft'
                )", new { handoverId, createdBy });

            result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });
        }

        return result;
    }

    public async Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            var handoverExists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", 
                new { handoverId }) > 0;
            
            if (!handoverExists)
            {
                return false;
            }

            const string sql = @"
                MERGE INTO HANDOVER_CONTENTS hc
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET SYNTHESIS = :content, SYNTHESIS_STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = LOCALTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, SYNTHESIS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT,
                            ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, PATIENT_SUMMARY_STATUS, SA_STATUS)
                    VALUES (:handoverId, :content, :status, :userId, LOCALTIMESTAMP,
                            'Stable', '', '', 'Draft', 'Draft')";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content = content ?? "", status, userId });
            return rowsAffected > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Situation Awareness

    public async Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT HANDOVER_ID as HandoverId, SITUATION_AWARENESS as Content, SA_STATUS as STATUS,
                   LAST_EDITED_BY as LastEditedBy, UPDATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM HANDOVER_CONTENTS
            WHERE HANDOVER_ID = :handoverId";

        var result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });

        if (result == null)
        {
            var createdBy = await conn.ExecuteScalarAsync<string>(
                "SELECT CREATED_BY_USER_ID FROM HANDOVERS WHERE ID = :handoverId",
                new { handoverId });

            if (createdBy == null) return null; 

            await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_CONTENTS (
                    HANDOVER_ID, SITUATION_AWARENESS, SA_STATUS, LAST_EDITED_BY, UPDATED_AT,
                    ILLNESS_SEVERITY, PATIENT_SUMMARY, SYNTHESIS, PATIENT_SUMMARY_STATUS, SYNTHESIS_STATUS
                ) VALUES (
                    :handoverId, '', 'Draft', :createdBy, LOCALTIMESTAMP,
                    'Stable', '', '', 'Draft', 'Draft'
                )", new { handoverId, createdBy });

            result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });
        }

        return result;
    }

    public async Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            var handoverExists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", 
                new { handoverId }) > 0;
            
            if (!handoverExists)
            {
                return false;
            }

            const string sql = @"
                MERGE INTO HANDOVER_CONTENTS hc
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET SITUATION_AWARENESS = :content, SA_STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = LOCALTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, SITUATION_AWARENESS, SA_STATUS, LAST_EDITED_BY, UPDATED_AT,
                            ILLNESS_SEVERITY, PATIENT_SUMMARY, SYNTHESIS, PATIENT_SUMMARY_STATUS, SYNTHESIS_STATUS)
                    VALUES (:handoverId, :content, :status, :userId, LOCALTIMESTAMP,
                            'Stable', '', '', 'Draft', 'Draft')";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content = content ?? "", status, userId });
            return rowsAffected > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Clinical Data

    public async Task<HandoverClinicalDataRecord?> GetClinicalDataAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT HANDOVER_ID as HandoverId, ILLNESS_SEVERITY as IllnessSeverity, 
                   PATIENT_SUMMARY as SummaryText, LAST_EDITED_BY as LastEditedBy, 
                   PATIENT_SUMMARY_STATUS as STATUS, UPDATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
            FROM HANDOVER_CONTENTS
            WHERE HANDOVER_ID = :handoverId";

        var result = await conn.QueryFirstOrDefaultAsync<HandoverClinicalDataRecord>(sql, new { handoverId });

        if (result == null)
        {
            var exists = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", new { handoverId }) > 0;
            if (!exists) return null;

            var userId = await conn.ExecuteScalarAsync<string>("SELECT CREATED_BY_USER_ID FROM HANDOVERS WHERE ID = :handoverId", new { handoverId });
            
            await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_CONTENTS (
                    HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS, LAST_EDITED_BY, UPDATED_AT,
                    SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS
                ) VALUES (
                    :handoverId, 'Stable', '', 'Draft', :userId, LOCALTIMESTAMP,
                    '', '', 'Draft', 'Draft'
                )",
                new { handoverId, userId });

            result = await conn.QueryFirstOrDefaultAsync<HandoverClinicalDataRecord>(sql, new { handoverId });
        }

        return result;
    }

    public async Task<bool> UpdateClinicalDataAsync(string handoverId, string illnessSeverity, string summaryText, string userId)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            
            // Get current summaryText to check if it changed
            var currentSummaryText = await conn.ExecuteScalarAsync<string>(
                "SELECT PATIENT_SUMMARY FROM HANDOVER_CONTENTS WHERE HANDOVER_ID = :handoverId",
                new { handoverId });
            
            // Only update UPDATED_AT if summaryText actually changed
            var summaryTextChanged = currentSummaryText != summaryText;
            
            const string sql = @"
                MERGE INTO HANDOVER_CONTENTS hc
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET ILLNESS_SEVERITY = :illnessSeverity, 
                               PATIENT_SUMMARY = :summaryText, 
                               LAST_EDITED_BY = :userId, 
                               UPDATED_AT = CASE WHEN :summaryTextChanged = 1 THEN LOCALTIMESTAMP ELSE hc.UPDATED_AT END
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS, LAST_EDITED_BY, UPDATED_AT,
                            SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS)
                    VALUES (:handoverId, :illnessSeverity, :summaryText, 'Draft', :userId, LOCALTIMESTAMP,
                            '', '', 'Draft', 'Draft')";

            var rowsAffected = await conn.ExecuteAsync(sql, new { 
                handoverId, 
                illnessSeverity, 
                summaryText, 
                userId,
                summaryTextChanged = summaryTextChanged ? 1 : 0
            });
            return rowsAffected > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion
}
