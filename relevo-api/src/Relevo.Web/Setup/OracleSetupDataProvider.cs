using System.Data;
using Dapper;
using Relevo.Infrastructure.Data.Oracle;
using Relevo.Web.Patients;
using Relevo.Web.Me;
using Relevo.Core.Interfaces;

// Use specific types from Core layer to avoid conflicts
using PatientRecord = Relevo.Core.Interfaces.PatientRecord;
using UnitRecord = Relevo.Core.Interfaces.UnitRecord;
using ShiftRecord = Relevo.Core.Interfaces.ShiftRecord;
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;

namespace Relevo.Web.Setup;

public class OracleSetupDataProvider(IOracleConnectionFactory _factory) : ISetupDataProvider
{
  // Using Oracle database for persistent assignments

  public IReadOnlyList<UnitRecord> GetUnits()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS ORDER BY ID";
    return conn.Query<UnitRecord>(sql).ToList();
  }

  public IReadOnlyList<ShiftRecord> GetShifts()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = @"SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS ORDER BY ID";
    return conn.Query<ShiftRecord>(sql).ToList();
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    const string countSql = "SELECT COUNT(1) FROM PATIENTS WHERE UNIT_ID = :unitId";
    const string pageSql = @"SELECT ID AS Id, NAME AS Name FROM (
      SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
      FROM PATIENTS WHERE UNIT_ID = :unitId
    ) WHERE RN BETWEEN :startRow AND :endRow";

    int total = conn.ExecuteScalar<int>(countSql, new { unitId });
    int startRow = ((p - 1) * ps) + 1;
    int endRow = p * ps;
    var items = conn.Query<PatientRecord>(pageSql, new { unitId, startRow, endRow }).ToList();
    return (items, total);
  }

  public void Assign(string userId, string shiftId, IEnumerable<string> patientIds)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // Remove existing assignments for this user
    conn.Execute("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :userId",
        new { userId });

    // Insert new assignments
    foreach (var patientId in patientIds)
    {
      conn.Execute(@"
        INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID)
        VALUES (:userId, :shiftId, :patientId)",
        new { userId, shiftId, patientId });
    }
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // Get total count of assigned patients
    const string countSql = "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
    int total = conn.ExecuteScalar<int>(countSql, new { userId });

    if (total == 0)
      return (Array.Empty<PatientRecord>(), 0);

    // Get assigned patients with pagination
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    int offset = (p - 1) * ps;

    const string patientsSql = @"
      SELECT p.ID AS Id, p.NAME AS Name
      FROM PATIENTS p
      INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
      WHERE ua.USER_ID = :userId
      ORDER BY p.ID
      OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

    var patients = conn.Query<PatientRecord>(patientsSql,
        new { userId, offset, pageSize });

    return (patients.ToList(), total);
  }

  public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // First, get the patient IDs assigned to this user from database
    const string patientIdsSql = "SELECT PATIENT_ID FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
    var patientIds = conn.Query<string>(patientIdsSql, new { userId }).ToArray();

    if (patientIds.Length == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    // Get total count of handovers for user's patients
    const string countSql = "SELECT COUNT(1) FROM HANDOVERS WHERE PATIENT_ID IN :patientIds";
    int total = conn.ExecuteScalar<int>(countSql, new { patientIds });

    if (total == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    // Get handovers with pagination
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    int offset = (p - 1) * ps;

    const string handoverSql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY, h.SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT
      FROM HANDOVERS h
      WHERE h.PATIENT_ID IN :patientIds
      ORDER BY h.CREATED_AT DESC
      OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

    var handoverRows = conn.Query(handoverSql, new { patientIds, offset, pageSize }).ToList();

    // Get action items for each handover
    var handovers = new List<HandoverRecord>();
    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;

      const string actionItemsSql = @"
        SELECT ID, DESCRIPTION, IS_COMPLETED
        FROM HANDOVER_ACTION_ITEMS
        WHERE HANDOVER_ID = :handoverId
        ORDER BY CREATED_AT";

      var actionItems = conn.Query(actionItemsSql, new { handoverId })
        .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
        .ToList();

      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        ActionItems: actionItems,
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        CreatedAt: row.CREATED_AT ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: row.READY_AT,
        StartedAt: row.STARTED_AT,
        AcknowledgedAt: row.ACKNOWLEDGED_AT,
        AcceptedAt: row.ACCEPTED_AT,
        CompletedAt: row.COMPLETED_AT,
        CancelledAt: row.CANCELLED_AT,
        RejectedAt: row.REJECTED_AT,
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: row.EXPIRED_AT,
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft"
      );

      handovers.Add(handover);
    }

    return (handovers, total);
  }

  public HandoverRecord? GetHandoverById(string handoverId)
  {
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY, h.SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT
      FROM HANDOVERS h
      LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      WHERE h.ID = :handoverId";

    var row = conn.QueryFirstOrDefault(sql, new { handoverId });

    if (row == null) return null;

    const string actionItemsSql = @"
      SELECT ID, DESCRIPTION, IS_COMPLETED
      FROM HANDOVER_ACTION_ITEMS
      WHERE HANDOVER_ID = :handoverId
      ORDER BY CREATED_AT";

    var actionItems = conn.Query(actionItemsSql, new { handoverId })
      .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
      .ToList();

    return new HandoverRecord(
      Id: row.ID,
      AssignmentId: row.ASSIGNMENT_ID ?? "",
      PatientId: row.PATIENT_ID,
      PatientName: row.PATIENT_NAME ?? "Unknown Patient",
      Status: row.STATUS,
      IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
      PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
      ActionItems: actionItems,
      SituationAwarenessDocId: null,
      Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
      ShiftName: row.SHIFT_NAME ?? "Unknown",
      CreatedBy: row.CREATED_BY ?? "system",
      AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
      ReceiverUserId: row.RECEIVER_USER_ID,
      CreatedAt: row.CREATED_AT ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
      ReadyAt: row.READY_AT,
      StartedAt: row.STARTED_AT,
      AcknowledgedAt: row.ACKNOWLEDGED_AT,
      AcceptedAt: row.ACCEPTED_AT,
      CompletedAt: row.COMPLETED_AT,
      CancelledAt: row.CANCELLED_AT,
      RejectedAt: row.REJECTED_AT,
      RejectionReason: row.REJECTION_REASON,
      ExpiredAt: row.EXPIRED_AT,
      HandoverType: row.HANDOVER_TYPE,
      HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
      FromShiftId: row.FROM_SHIFT_ID,
      ToShiftId: row.TO_SHIFT_ID,
      ToDoctorId: row.TO_DOCTOR_ID,
      StateName: row.STATENAME ?? "Draft"
    );
  }

  public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    // Generate IDs
    var handoverId = $"handover-{Guid.NewGuid().ToString().Substring(0, 8)}";
    var assignmentId = $"assign-{Guid.NewGuid().ToString().Substring(0, 8)}";

    // Create user assignment if it doesn't exist
    conn.Execute(@"
      INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
      VALUES (:assignmentId, :toDoctorId, :toShiftId, :patientId, SYSTIMESTAMP)
      ON DUPLICATE KEY UPDATE ASSIGNED_AT = SYSTIMESTAMP",
      new { assignmentId, toDoctorId = request.ToDoctorId, toShiftId = request.ToShiftId, patientId = request.PatientId });

    // Create handover
    conn.Execute(@"
      INSERT INTO HANDOVERS (
        ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
        SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
        CREATED_BY, CREATED_AT, INITIATED_AT
      ) VALUES (
        :handoverId, :assignmentId, :patientId, 'Active', 'Stable',
        :shiftName, :fromShiftId, :toShiftId, :fromDoctorId, :toDoctorId,
        :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP
      )",
      new {
        handoverId,
        assignmentId,
        patientId = request.PatientId,
        shiftName = $"{request.FromShiftId} → {request.ToShiftId}",
        fromShiftId = request.FromShiftId,
        toShiftId = request.ToShiftId,
        fromDoctorId = request.FromDoctorId,
        toDoctorId = request.ToDoctorId,
        initiatedBy = request.InitiatedBy
      });

    // Add participants
    conn.Execute(@"
      INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS, JOINED_AT)
      SELECT :participantId1, :handoverId, :fromDoctorId, u.FULL_NAME, 'Handing Over Doctor', 'active', SYSTIMESTAMP
      FROM USERS u WHERE u.ID = :fromDoctorId
      UNION ALL
      SELECT :participantId2, :handoverId, :toDoctorId, u.FULL_NAME, 'Receiving Doctor', 'active', SYSTIMESTAMP
      FROM USERS u WHERE u.ID = :toDoctorId",
      new {
        handoverId,
        fromDoctorId = request.FromDoctorId,
        toDoctorId = request.ToDoctorId,
        participantId1 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}",
        participantId2 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
      });

    // Create default sections
    conn.Execute(@"
      INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, CREATED_AT)
      VALUES
      (:illnessId, :handoverId, 'illness_severity', 'Stable - Patient condition stable', 'draft', SYSTIMESTAMP),
      (:patientId, :handoverId, 'patient_summary', '', 'draft', SYSTIMESTAMP),
      (:actionsId, :handoverId, 'action_items', '', 'draft', SYSTIMESTAMP),
      (:awarenessId, :handoverId, 'situation_awareness', '', 'draft', SYSTIMESTAMP),
      (:synthesisId, :handoverId, 'synthesis', '', 'draft', SYSTIMESTAMP)",
      new {
        handoverId,
        illnessId = $"section-{Guid.NewGuid().ToString().Substring(0, 8)}",
        patientId = $"section-{Guid.NewGuid().ToString().Substring(0, 8)}",
        actionsId = $"section-{Guid.NewGuid().ToString().Substring(0, 8)}",
        awarenessId = $"section-{Guid.NewGuid().ToString().Substring(0, 8)}",
        synthesisId = $"section-{Guid.NewGuid().ToString().Substring(0, 8)}"
      });

    // Return the created handover
    return GetHandoverById(handoverId) ?? throw new InvalidOperationException("Failed to create handover");
  }

  public async Task<bool> AcceptHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    var affected = conn.Execute(@"
      UPDATE HANDOVERS
      SET ACCEPTED_AT = SYSTIMESTAMP
      WHERE ID = :handoverId AND TO_DOCTOR_ID = :userId AND READY_AT IS NOT NULL AND ACCEPTED_AT IS NULL",
      new { handoverId, userId });

    return affected > 0;
  }

  public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    var affected = conn.Execute(@"
      UPDATE HANDOVERS
      SET STATUS = 'Completed', COMPLETED_AT = SYSTIMESTAMP, COMPLETED_BY = :userId
      WHERE ID = :handoverId AND TO_DOCTOR_ID = :userId AND STATUS = 'InProgress'",
      new { handoverId, userId });

    return affected > 0;
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY, h.SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT
      FROM HANDOVERS h
      INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      WHERE h.TO_DOCTOR_ID = :userId
        AND h.READY_AT IS NOT NULL
        AND h.ACCEPTED_AT IS NULL
        AND h.COMPLETED_AT IS NULL
        AND h.CANCELLED_AT IS NULL
        AND h.REJECTED_AT IS NULL
        AND h.EXPIRED_AT IS NULL
      ORDER BY h.INITIATED_AT DESC";

    var handoverRows = conn.Query(sql, new { userId }).ToList();
    var handovers = new List<HandoverRecord>();

    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;

      const string actionItemsSql = @"
        SELECT ID, DESCRIPTION, IS_COMPLETED
        FROM HANDOVER_ACTION_ITEMS
        WHERE HANDOVER_ID = :handoverId
        ORDER BY CREATED_AT";

      var actionItems = conn.Query(actionItemsSql, new { handoverId })
        .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
        .ToList();

      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        ActionItems: actionItems,
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        CreatedAt: row.CREATED_AT ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: row.READY_AT,
        StartedAt: row.STARTED_AT,
        AcknowledgedAt: row.ACKNOWLEDGED_AT,
        AcceptedAt: row.ACCEPTED_AT,
        CompletedAt: row.COMPLETED_AT,
        CancelledAt: row.CANCELLED_AT,
        RejectedAt: row.REJECTED_AT,
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: row.EXPIRED_AT,
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft"
      );

      handovers.Add(handover);
    }

    return handovers;
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY, h.SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT
      FROM HANDOVERS h
      INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      WHERE h.PATIENT_ID = :patientId
      ORDER BY h.CREATED_AT DESC";

    var handoverRows = conn.Query(sql, new { patientId }).ToList();
    var handovers = new List<HandoverRecord>();

    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;

      const string actionItemsSql = @"
        SELECT ID, DESCRIPTION, IS_COMPLETED
        FROM HANDOVER_ACTION_ITEMS
        WHERE HANDOVER_ID = :handoverId
        ORDER BY CREATED_AT";

      var actionItems = conn.Query(actionItemsSql, new { handoverId })
        .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
        .ToList();

      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        ActionItems: actionItems,
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        CreatedAt: row.CREATED_AT ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: row.READY_AT,
        StartedAt: row.STARTED_AT,
        AcknowledgedAt: row.ACKNOWLEDGED_AT,
        AcceptedAt: row.ACCEPTED_AT,
        CompletedAt: row.COMPLETED_AT,
        CancelledAt: row.CANCELLED_AT,
        RejectedAt: row.REJECTED_AT,
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: row.EXPIRED_AT,
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft"
      );

      handovers.Add(handover);
    }

    return handovers;
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY, h.SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT
      FROM HANDOVERS h
      INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      WHERE h.FROM_DOCTOR_ID = :fromDoctorId AND h.TO_DOCTOR_ID = :toDoctorId
      ORDER BY h.INITIATED_AT DESC";

    var handoverRows = conn.Query(sql, new { fromDoctorId, toDoctorId }).ToList();
    var handovers = new List<HandoverRecord>();

    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;

      const string actionItemsSql = @"
        SELECT ID, DESCRIPTION, IS_COMPLETED
        FROM HANDOVER_ACTION_ITEMS
        WHERE HANDOVER_ID = :handoverId
        ORDER BY CREATED_AT";

      var actionItems = conn.Query(actionItemsSql, new { handoverId })
        .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
        .ToList();

      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        ActionItems: actionItems,
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        CreatedAt: row.CREATED_AT ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: row.READY_AT,
        StartedAt: row.STARTED_AT,
        AcknowledgedAt: row.ACKNOWLEDGED_AT,
        AcceptedAt: row.ACCEPTED_AT,
        CompletedAt: row.COMPLETED_AT,
        CancelledAt: row.CANCELLED_AT,
        RejectedAt: row.REJECTED_AT,
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: row.EXPIRED_AT,
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft"
      );

      handovers.Add(handover);
    }

    return handovers;
  }
}


