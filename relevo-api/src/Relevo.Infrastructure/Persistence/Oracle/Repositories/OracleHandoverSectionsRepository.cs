using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverSectionsRepository : IHandoverSectionsRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverSectionsRepository> _logger;

    public OracleHandoverSectionsRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverSectionsRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<HandoverPatientDataRecord?> GetPatientDataAsync(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT HANDOVER_ID as HandoverId, ILLNESS_SEVERITY as IllnessSeverity, SUMMARY_TEXT as SummaryText,
                       LAST_EDITED_BY as LastEditedBy, STATUS, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_PATIENT_DATA
                WHERE HANDOVER_ID = :handoverId";
            
            return await conn.QueryFirstOrDefaultAsync<HandoverPatientDataRecord>(sql, new { handoverId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patient data for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // First check if the handover exists
            var handoverExists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM HANDOVERS WHERE ID = :handoverId",
                new { handoverId });

            // If handover doesn't exist, return null
            if (handoverExists == 0)
            {
                return null;
            }

            const string sql = @"
                SELECT HANDOVER_ID as HandoverId, CONTENT as Content, STATUS,
                       LAST_EDITED_BY as LastEditedBy, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_SITUATION_AWARENESS
                WHERE HANDOVER_ID = :handoverId";

            var result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });

            // If no record exists, create a default one
            if (result == null)
            {
                // Get the handover's created_by to use as last_edited_by
                var createdBy = await conn.ExecuteScalarAsync<string>(
                    "SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId",
                    new { handoverId });

                await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_SITUATION_AWARENESS (
                    HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
                ) VALUES (
                    :handoverId, '', 'Draft', :createdBy, SYSDATE, SYSDATE
                )", new { handoverId, createdBy });

                // Return the newly created record
                result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get situation awareness for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT HANDOVER_ID as HandoverId, CONTENT as Content, STATUS,
                       LAST_EDITED_BY as LastEditedBy, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_SYNTHESIS
                WHERE HANDOVER_ID = :handoverId";

            var result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });

            // If no record exists, create a default one
            if (result == null)
            {
                // Get the handover's created_by to use as last_edited_by
                var createdBy = await conn.ExecuteScalarAsync<string>(
                    "SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId",
                    new { handoverId });

                await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_SYNTHESIS (
                    HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
                ) VALUES (
                    :handoverId, '', 'Draft', :createdBy, SYSDATE, SYSDATE
                )", new { handoverId, createdBy });

                // Return the newly created record
                result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get synthesis for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> UpdatePatientDataAsync(string handoverId, string illnessSeverity, string? summaryText, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                MERGE INTO HANDOVER_PATIENT_DATA pd
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (pd.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET
                        ILLNESS_SEVERITY = :illnessSeverity,
                        SUMMARY_TEXT = :summaryText,
                        STATUS = :status,
                        LAST_EDITED_BY = :userId,
                        UPDATED_AT = SYSTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, UPDATED_AT)
                    VALUES (:handoverId, :illnessSeverity, :summaryText, :status, :userId, SYSTIMESTAMP)";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, illnessSeverity, summaryText, status, userId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update patient data for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                MERGE INTO HANDOVER_SITUATION_AWARENESS s
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (s.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, UPDATED_AT)
                    VALUES (:handoverId, :content, :status, :userId, SYSTIMESTAMP)";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content, status, userId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update situation awareness for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                MERGE INTO HANDOVER_SYNTHESIS s
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (s.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, UPDATED_AT)
                    VALUES (:handoverId, :content, :status, :userId, SYSTIMESTAMP)";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content, status, userId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update synthesis for handover {HandoverId}", handoverId);
            throw;
        }
    }
}
