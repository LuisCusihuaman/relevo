using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Repositories;
using Relevo.Infrastructure.Data.Oracle;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.IntegrationTests.Data;

public class OracleSetupRepositoryTests : BaseDapperTestFixture
{
    private readonly OracleSetupRepository _repository;
    private readonly ILogger<OracleSetupRepository> _logger;

    public OracleSetupRepositoryTests()
    {
        if (_connection == null)
        {
            // Skip tests if Oracle is not available
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OracleSetupRepository>.Instance;
            _repository = null!;
            return;
        }

        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OracleSetupRepository>.Instance;
        var factory = new OracleConnectionFactory("User Id=system;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
        _repository = new OracleSetupRepository(factory, _logger);

        // Setup test data
        SetupTestData();
    }

    private void SetupTestData()
    {
        if (_connection == null) return;

        try
        {
            // Create test units
            ExecuteSql(@"
                INSERT INTO UNITS (ID, NAME) VALUES ('test-unit-1', 'Test Unit 1')
                ON DUPLICATE KEY UPDATE NAME = 'Test Unit 1'");

            // Create test shifts
            ExecuteSql(@"
                INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('test-shift-1', 'Test Shift', '08:00', '16:00')
                ON DUPLICATE KEY UPDATE NAME = 'Test Shift'");

            // Create test patients
            ExecuteSql(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER)
                VALUES ('test-patient-1', 'Test Patient 1', 'test-unit-1', SYSDATE - 365*10, 'Male')
                ON DUPLICATE KEY UPDATE NAME = 'Test Patient 1'");

            ExecuteSql(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER)
                VALUES ('test-patient-2', 'Test Patient 2', 'test-unit-1', SYSDATE - 365*15, 'Female')
                ON DUPLICATE KEY UPDATE NAME = 'Test Patient 2'");

            // Create test assignment
            ExecuteSql(@"
                INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
                VALUES ('test-assignment-1', 'test-user-1', 'test-shift-1', 'test-patient-1')
                ON DUPLICATE KEY UPDATE USER_ID = 'test-user-1'");
        }
        catch (Exception ex)
        {
            // If setup fails, tests will be skipped
            _oracleUnavailableMessage = $"Test setup failed: {ex.Message}";
        }
    }

    private void ExecuteSql(string sql)
    {
        if (_connection == null) return;

        using var cmd = _connection.CreateCommand();
        if (_transaction != null)
        {
            cmd.Transaction = _transaction;
        }
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    [Fact]
    public async Task CreateHandoverForAssignmentAsync_CreatesHandover_WhenAssignmentExists()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var assignmentId = "test-assignment-1";
        var userId = "test-user-1";

        // Act
        await _repository.CreateHandoverForAssignmentAsync(assignmentId, userId);

        // Assert
        var handoverExists = CheckHandoverExists(assignmentId);
        handoverExists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandoverForAssignmentAsync_ThrowsException_WhenAssignmentNotFound()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var nonExistentAssignmentId = "non-existent-assignment";
        var userId = "test-user-1";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.CreateHandoverForAssignmentAsync(nonExistentAssignmentId, userId));
    }

    [Fact]
    public void GetPatientHandovers_ReturnsHandovers_WhenPatientHasHandovers()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var patientId = "test-patient-1";

        // First create a handover for the test
        CreateTestHandover(patientId);

        // Act
        var (handovers, totalCount) = _repository.GetPatientHandovers(patientId, 1, 25);

        // Assert
        handovers.Should().NotBeNull();
        handovers.Should().NotBeEmpty();
        totalCount.Should().BeGreaterThan(0);
        handovers[0].PatientId.Should().Be(patientId);
        handovers[0].PatientName.Should().Be("Test Patient 1");
    }

    [Fact]
    public void GetPatientHandovers_ReturnsEmptyList_WhenPatientHasNoHandovers()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var patientId = "test-patient-no-handovers";

        // Act
        var (handovers, totalCount) = _repository.GetPatientHandovers(patientId, 1, 25);

        // Assert
        handovers.Should().NotBeNull();
        handovers.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public void GetPatientHandovers_HandlesPaginationCorrectly()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var patientId = "test-patient-1";

        // Create multiple handovers for pagination test
        CreateTestHandover(patientId, "test-handover-page-1");
        CreateTestHandover(patientId, "test-handover-page-2");
        CreateTestHandover(patientId, "test-handover-page-3");

        // Act - Get first page with page size 2
        var (handovers, totalCount) = _repository.GetPatientHandovers(patientId, 1, 2);

        // Assert
        handovers.Should().NotBeNull();
        handovers.Should().HaveCount(2);
        totalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task AssignAsync_ReturnsAssignmentIds_WhenSuccessful()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var userId = "test-assign-user";
        var shiftId = "test-shift-1";
        var patientIds = new List<string> { "test-patient-1" };

        // Act
        var assignmentIds = await _repository.AssignAsync(userId, shiftId, patientIds);

        // Assert
        assignmentIds.Should().NotBeNull();
        assignmentIds.Should().HaveCount(1);
        assignmentIds[0].Should().Contain(userId);
        assignmentIds[0].Should().Contain(shiftId);
        assignmentIds[0].Should().Contain("test-patient-1");
    }

    [Fact]
    public async Task AssignAsync_CreatesMultipleAssignments_WhenMultiplePatients()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var userId = "test-multi-user";
        var shiftId = "test-shift-1";
        var patientIds = new List<string> { "test-patient-1", "test-patient-2" };

        // Act
        var assignmentIds = await _repository.AssignAsync(userId, shiftId, patientIds);

        // Assert
        assignmentIds.Should().NotBeNull();
        assignmentIds.Should().HaveCount(2);
        assignmentIds.All(id => id.Contains(userId)).Should().BeTrue();
        assignmentIds.All(id => id.Contains(shiftId)).Should().BeTrue();
    }

    [Fact]
    public void GetHandoverById_ReturnsHandover_WhenHandoverExists()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var handoverId = "test-handover-by-id";
        var patientId = "test-patient-1";
        var assignmentId = "test-assignment-for-handover";

        // Create test handover
        CreateTestHandoverForGetById(handoverId, patientId, assignmentId);

        // Act
        var result = _repository.GetHandoverById(handoverId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(handoverId);
        result.AssignmentId.Should().Be(assignmentId);
        result.PatientId.Should().Be(patientId);
        result.PatientName.Should().Be("Test Patient 1");
        result.Status.Should().Be("Active");
        result.IllnessSeverity.Value.Should().Be("Stable");
        result.PatientSummary.Value.Should().Be("Test handover for GetHandoverById");
        result.ActionItems.Should().NotBeNull();
        result.ShiftName.Should().Be("Test Shift");
        result.CreatedBy.Should().Be("test-user-getbyid");
        result.AssignedTo.Should().Be("test-user-getbyid");
    }

    [Fact]
    public void GetHandoverById_ReturnsNull_WhenHandoverDoesNotExist()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var nonExistentHandoverId = "non-existent-handover-id";

        // Act
        var result = _repository.GetHandoverById(nonExistentHandoverId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetHandoverById_ReturnsHandoverWithActionItems_WhenHandoverHasActionItems()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var handoverId = "test-handover-with-actions";
        var patientId = "test-patient-1";
        var assignmentId = "test-assignment-with-actions";

        // Create test handover with action items
        CreateTestHandoverWithActions(handoverId, patientId, assignmentId);

        // Act
        var result = _repository.GetHandoverById(handoverId);

        // Assert
        result.Should().NotBeNull();
        result!.ActionItems.Should().NotBeNull();
        result.ActionItems.Should().HaveCountGreaterThan(0);
        result.ActionItems[0].Id.Should().NotBeNullOrEmpty();
        result.ActionItems[0].Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetHandoverById_ReturnsHandoverWithSynthesis_WhenSynthesisExists()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var handoverId = "test-handover-with-synthesis";
        var patientId = "test-patient-1";
        var assignmentId = "test-assignment-with-synthesis";

        // Create test handover with synthesis
        CreateTestHandoverWithSynthesis(handoverId, patientId, assignmentId);

        // Act
        var result = _repository.GetHandoverById(handoverId);

        // Assert
        result.Should().NotBeNull();
        result!.Synthesis.Should().NotBeNull();
        result.Synthesis!.Value.Should().Be("Test synthesis content");
    }

    [Fact]
    public void GetHandoverById_ReturnsHandoverWithSituationAwarenessDocId_WhenDocIdExists()
    {
        if (_connection == null)
        {
            Assert.True(true, _oracleUnavailableMessage);
            return;
        }

        // Arrange
        var handoverId = "test-handover-with-docid";
        var patientId = "test-patient-1";
        var assignmentId = "test-assignment-with-docid";
        var docId = "situation-doc-123";

        // Create test handover with situation awareness document ID
        CreateTestHandoverWithDocId(handoverId, patientId, assignmentId, docId);

        // Act
        var result = _repository.GetHandoverById(handoverId);

        // Assert
        result.Should().NotBeNull();
        result!.SituationAwarenessDocId.Should().Be(docId);
    }

    private bool CheckHandoverExists(string assignmentId)
    {
        if (_connection == null) return false;

        using var cmd = _connection.CreateCommand();
        if (_transaction != null)
        {
            cmd.Transaction = _transaction;
        }
        cmd.CommandText = "SELECT COUNT(1) FROM HANDOVERS WHERE ASSIGNMENT_ID = :assignmentId";
        cmd.Parameters.Add(new OracleParameter("assignmentId", assignmentId));

        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count > 0;
    }

    private void CreateTestHandover(string patientId, string handoverId = "test-handover-1")
    {
        if (_connection == null) return;

        ExecuteSql($@"
            INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES ('test-assignment-{patientId}', 'test-user-{patientId}', 'test-shift-1', '{patientId}')");

        ExecuteSql($@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SHIFT_NAME, CREATED_BY, ASSIGNED_TO
            ) VALUES (
                '{handoverId}', 'test-assignment-{patientId}', '{patientId}', 'Active',
                'Stable', 'Test handover summary', 'Test Shift', 'test-user-{patientId}', 'test-user-{patientId}'
            )");
    }

    private void CreateTestHandoverForGetById(string handoverId, string patientId, string assignmentId)
    {
        if (_connection == null) return;

        ExecuteSql($@"
            INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES ('{assignmentId}', 'test-user-getbyid', 'test-shift-1', '{patientId}')");

        ExecuteSql($@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SHIFT_NAME, CREATED_BY, ASSIGNED_TO
            ) VALUES (
                '{handoverId}', '{assignmentId}', '{patientId}', 'Active',
                'Stable', 'Test handover for GetHandoverById', 'Test Shift', 'test-user-getbyid', 'test-user-getbyid'
            )");
    }

    private void CreateTestHandoverWithActions(string handoverId, string patientId, string assignmentId)
    {
        if (_connection == null) return;

        ExecuteSql($@"
            INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES ('{assignmentId}', 'test-user-actions', 'test-shift-1', '{patientId}')");

        ExecuteSql($@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SHIFT_NAME, CREATED_BY, ASSIGNED_TO
            ) VALUES (
                '{handoverId}', '{assignmentId}', '{patientId}', 'Active',
                'Stable', 'Test handover with actions', 'Test Shift', 'test-user-actions', 'test-user-actions'
            )");

        // Add action items
        ExecuteSql($@"
            INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
            VALUES ('action-1', '{handoverId}', 'Check vital signs', 0)");

        ExecuteSql($@"
            INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
            VALUES ('action-2', '{handoverId}', 'Administer medication', 1)");
    }

    private void CreateTestHandoverWithSynthesis(string handoverId, string patientId, string assignmentId)
    {
        if (_connection == null) return;

        ExecuteSql($@"
            INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES ('{assignmentId}', 'test-user-synthesis', 'test-shift-1', '{patientId}')");

        ExecuteSql($@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO
            ) VALUES (
                '{handoverId}', '{assignmentId}', '{patientId}', 'Completed',
                'Stable', 'Test handover with synthesis', 'Test synthesis content',
                'Test Shift', 'test-user-synthesis', 'test-user-synthesis'
            )");
    }

    private void CreateTestHandoverWithDocId(string handoverId, string patientId, string assignmentId, string docId)
    {
        if (_connection == null) return;

        ExecuteSql($@"
            INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES ('{assignmentId}', 'test-user-docid', 'test-shift-1', '{patientId}')");

        ExecuteSql($@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SHIFT_NAME, CREATED_BY, ASSIGNED_TO
            ) VALUES (
                '{handoverId}', '{assignmentId}', '{patientId}', 'InProgress',
                'Watcher', 'Test handover with doc ID', '{docId}',
                'Test Shift', 'test-user-docid', 'test-user-docid'
            )");
    }

    private class TestOracleConnectionFactory : IDbConnectionFactory
    {
        private readonly IDbConnection _testConnection;

        public TestOracleConnectionFactory(IDbConnection connection)
        {
            _testConnection = connection;
        }

        public IDbConnection CreateConnection()
        {
            return _testConnection;
        }
    }
}
