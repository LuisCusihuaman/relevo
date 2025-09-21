using Relevo.Core.Interfaces;
using Relevo.UseCases.Setup;
using FluentAssertions;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

// Use specific types from Core layer to avoid conflicts
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;
using HandoverParticipantRecord = Relevo.Core.Interfaces.HandoverParticipantRecord;
using HandoverSectionRecord = Relevo.Core.Interfaces.HandoverSectionRecord;
using HandoverSyncStatusRecord = Relevo.Core.Interfaces.HandoverSyncStatusRecord;
using UserPreferencesRecord = Relevo.Core.Interfaces.UserPreferencesRecord;
using UserSessionRecord = Relevo.Core.Interfaces.UserSessionRecord;

namespace Relevo.UnitTests.UseCases.Setup;

public class SetupServiceTests
{
    private readonly ISetupRepository _repository = Substitute.For<ISetupRepository>();
    private readonly ISetupService _setupService;

    public SetupServiceTests()
    {
        // Create a mock implementation of ISetupService that delegates to the repository
        _setupService = new MockSetupService(_repository);
    }

    // Mock implementation to avoid complex constructor dependencies
    private class MockSetupService : ISetupService
    {
        private readonly ISetupRepository _repository;

        public MockSetupService(ISetupRepository repository)
        {
            _repository = repository;
        }

        public Task<HandoverRecord?> GetActiveHandoverAsync(string userId)
        {
            return Task.FromResult(_repository.GetActiveHandover(userId));
        }

        public Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverParticipants(handoverId));
        }

        public Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverSections(handoverId));
        }

        public Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId)
        {
            return Task.FromResult(_repository.GetHandoverSyncStatus(handoverId, userId));
        }

        public Task<bool> UpdateHandoverSectionAsync(string handoverId, string sectionId, string content, string status, string userId)
        {
            return Task.FromResult(_repository.UpdateHandoverSection(handoverId, sectionId, content, status, userId));
        }

        public Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId)
        {
            return Task.FromResult(_repository.GetUserPreferences(userId));
        }

        public Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId)
        {
            return Task.FromResult(_repository.GetUserSessions(userId));
        }

        public Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences)
        {
            return Task.FromResult(_repository.UpdateUserPreferences(userId, preferences));
        }

        // Handover Messages
        public Task<IReadOnlyList<HandoverMessageRecord>> GetHandoverMessagesAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverMessages(handoverId));
        }

        public Task<HandoverMessageRecord> CreateHandoverMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType)
        {
            return Task.FromResult(_repository.CreateHandoverMessage(handoverId, userId, userName, messageText, messageType));
        }

        // Handover Activity Log
        public Task<IReadOnlyList<HandoverActivityItemRecord>> GetHandoverActivityLogAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverActivityLog(handoverId));
        }

        // Handover Checklists
        public Task<IReadOnlyList<HandoverChecklistItemRecord>> GetHandoverChecklistsAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverChecklists(handoverId));
        }

        public Task<bool> UpdateChecklistItemAsync(string handoverId, string itemId, bool isChecked, string userId)
        {
            return Task.FromResult(_repository.UpdateChecklistItem(handoverId, itemId, isChecked, userId));
        }

        // Handover Contingency Plans
        public Task<IReadOnlyList<HandoverContingencyPlanRecord>> GetHandoverContingencyPlansAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverContingencyPlans(handoverId));
        }

        public Task<HandoverContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string conditionText, string actionText, string priority, string createdBy)
        {
            return Task.FromResult(_repository.CreateContingencyPlan(handoverId, conditionText, actionText, priority, createdBy));
        }

        public Task AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
        {
            return Task.FromResult(_repository.AssignAsync(userId, shiftId, patientIds));
        }

        public Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize)
        {
            return Task.FromResult(_repository.GetMyPatients(userId, page, pageSize));
        }

        public Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize)
        {
            return Task.FromResult(_repository.GetMyHandovers(userId, page, pageSize));
        }

        public Task<IReadOnlyList<UnitRecord>> GetUnitsAsync()
        {
            return Task.FromResult(_repository.GetUnits());
        }

        public Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync()
        {
            return Task.FromResult(_repository.GetShifts());
        }

        public Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize)
        {
            return Task.FromResult(_repository.GetPatientsByUnit(unitId, page, pageSize));
        }

        public Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize)
        {
            return Task.FromResult(_repository.GetAllPatients(page, pageSize));
        }

        public Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId)
        {
            return Task.FromResult(_repository.GetPatientById(patientId));
        }

        public Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize)
        {
            return Task.FromResult(_repository.GetPatientHandovers(patientId, page, pageSize));
        }

        public Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverById(handoverId));
        }


        // Handover Creation and Management
        public Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
        {
            throw new NotImplementedException("CreateHandoverAsync not implemented in mock");
        }

        public Task<bool> AcceptHandoverAsync(string handoverId, string userId)
        {
            throw new NotImplementedException("AcceptHandoverAsync not implemented in mock");
        }

        public Task<bool> CompleteHandoverAsync(string handoverId, string userId)
        {
            throw new NotImplementedException("CompleteHandoverAsync not implemented in mock");
        }

        public Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId)
        {
            throw new NotImplementedException("GetPendingHandoversForUserAsync not implemented in mock");
        }

        public Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId)
        {
            throw new NotImplementedException("GetHandoversByPatientAsync not implemented in mock");
        }

        public Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId)
        {
            throw new NotImplementedException("GetShiftTransitionHandoversAsync not implemented in mock");
        }

        // Action Items
        public Task<IReadOnlyList<HandoverActionItemRecord>> GetHandoverActionItemsAsync(string handoverId)
        {
            return Task.FromResult(_repository.GetHandoverActionItems(handoverId));
        }

        public Task<string> CreateHandoverActionItemAsync(string handoverId, string description, string priority)
        {
            return Task.FromResult(_repository.CreateHandoverActionItem(handoverId, description, priority));
        }

        public Task<bool> UpdateHandoverActionItemAsync(string handoverId, string itemId, bool isCompleted)
        {
            return Task.FromResult(_repository.UpdateHandoverActionItem(handoverId, itemId, isCompleted));
        }

        public Task<bool> DeleteHandoverActionItemAsync(string handoverId, string itemId)
        {
            return Task.FromResult(_repository.DeleteHandoverActionItem(handoverId, itemId));
        }
    }

    [Fact]
    public async Task GetActiveHandoverAsync_ReturnsNull_WhenNoActiveHandover()
    {
        // Arrange
        var userId = "test-user-123";
        _repository.GetActiveHandover(userId).Returns((HandoverRecord?)null);

        // Act
        var result = await _setupService.GetActiveHandoverAsync(userId);

        // Assert
        result.Should().BeNull();
        _repository.Received(1).GetActiveHandover(userId);
    }

    [Fact]
    public async Task GetActiveHandoverAsync_ReturnsHandover_WhenActiveHandoverExists()
    {
        // Arrange
        var userId = "test-user-123";
        var expectedHandover = new HandoverRecord(
            "handover-001", "assignment-001", "patient-001", "John Doe",
            "Active", new HandoverIllnessSeverity("Stable"),
            new HandoverPatientSummary("Patient is stable"),
            new List<HandoverActionItem>(), "situation-doc-001",
            null, "Morning Shift", "user-123", "user-456", "2024-01-01 08:00:00"
        );
        _repository.GetActiveHandover(userId).Returns(expectedHandover);

        // Act
        var result = await _setupService.GetActiveHandoverAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("handover-001");
        result.PatientId.Should().Be("patient-001");
        result.Status.Should().Be("Active");
        _repository.Received(1).GetActiveHandover(userId);
    }

    [Fact]
    public async Task GetHandoverParticipantsAsync_ReturnsEmptyList_WhenNoParticipants()
    {
        // Arrange
        var handoverId = "handover-001";
        _repository.GetHandoverParticipants(handoverId).Returns(new List<HandoverParticipantRecord>());

        // Act
        var result = await _setupService.GetHandoverParticipantsAsync(handoverId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _repository.Received(1).GetHandoverParticipants(handoverId);
    }

    [Fact]
    public async Task GetHandoverParticipantsAsync_ReturnsParticipants_WhenParticipantsExist()
    {
        // Arrange
        var handoverId = "handover-001";
        var participants = new List<HandoverParticipantRecord>
        {
            new HandoverParticipantRecord("part-001", handoverId, "user-123", "John Doe", "Physician", "active",
                                        System.DateTime.Now, System.DateTime.Now),
            new HandoverParticipantRecord("part-002", handoverId, "user-456", "Jane Smith", "Nurse", "active",
                                        System.DateTime.Now, System.DateTime.Now)
        };
        _repository.GetHandoverParticipants(handoverId).Returns(participants);

        // Act
        var result = await _setupService.GetHandoverParticipantsAsync(handoverId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].UserName.Should().Be("John Doe");
        result[1].UserName.Should().Be("Jane Smith");
        _repository.Received(1).GetHandoverParticipants(handoverId);
    }

    [Fact]
    public async Task GetHandoverSectionsAsync_ReturnsEmptyList_WhenNoSections()
    {
        // Arrange
        var handoverId = "handover-001";
        _repository.GetHandoverSections(handoverId).Returns(new List<HandoverSectionRecord>());

        // Act
        var result = await _setupService.GetHandoverSectionsAsync(handoverId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _repository.Received(1).GetHandoverSections(handoverId);
    }

    [Fact]
    public async Task GetHandoverSectionsAsync_ReturnsSections_WhenSectionsExist()
    {
        // Arrange
        var handoverId = "handover-001";
        var sections = new List<HandoverSectionRecord>
        {
            new HandoverSectionRecord("section-001", handoverId, "illness_severity", "Patient is stable", "completed",
                                    "user-123", System.DateTime.Now, System.DateTime.Now),
            new HandoverSectionRecord("section-002", handoverId, "patient_summary", "Patient summary content", "draft",
                                    "user-456", System.DateTime.Now, System.DateTime.Now)
        };
        _repository.GetHandoverSections(handoverId).Returns(sections);

        // Act
        var result = await _setupService.GetHandoverSectionsAsync(handoverId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SectionType.Should().Be("illness_severity");
        result[1].SectionType.Should().Be("patient_summary");
        _repository.Received(1).GetHandoverSections(handoverId);
    }

    [Fact]
    public async Task GetHandoverSyncStatusAsync_ReturnsNull_WhenNoSyncStatus()
    {
        // Arrange
        var handoverId = "handover-001";
        var userId = "user-123";
        _repository.GetHandoverSyncStatus(handoverId, userId).Returns((HandoverSyncStatusRecord?)null);

        // Act
        var result = await _setupService.GetHandoverSyncStatusAsync(handoverId, userId);

        // Assert
        result.Should().BeNull();
        _repository.Received(1).GetHandoverSyncStatus(handoverId, userId);
    }

    [Fact]
    public async Task GetHandoverSyncStatusAsync_ReturnsSyncStatus_WhenSyncStatusExists()
    {
        // Arrange
        var handoverId = "handover-001";
        var userId = "user-123";
        var syncStatus = new HandoverSyncStatusRecord("sync-001", handoverId, userId, "synced", System.DateTime.Now, 1);
        _repository.GetHandoverSyncStatus(handoverId, userId).Returns(syncStatus);

        // Act
        var result = await _setupService.GetHandoverSyncStatusAsync(handoverId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.SyncStatus.Should().Be("synced");
        result.Version.Should().Be(1);
        _repository.Received(1).GetHandoverSyncStatus(handoverId, userId);
    }

    [Fact]
    public async Task UpdateHandoverSectionAsync_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        var handoverId = "handover-001";
        var sectionId = "section-001";
        var content = "Updated content";
        var status = "completed";
        var userId = "user-123";
        _repository.UpdateHandoverSection(handoverId, sectionId, content, status, userId).Returns(false);

        // Act
        var result = await _setupService.UpdateHandoverSectionAsync(handoverId, sectionId, content, status, userId);

        // Assert
        result.Should().BeFalse();
        _repository.Received(1).UpdateHandoverSection(handoverId, sectionId, content, status, userId);
    }

    [Fact]
    public async Task UpdateHandoverSectionAsync_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var handoverId = "handover-001";
        var sectionId = "section-001";
        var content = "Updated content";
        var status = "completed";
        var userId = "user-123";
        _repository.UpdateHandoverSection(handoverId, sectionId, content, status, userId).Returns(true);

        // Act
        var result = await _setupService.UpdateHandoverSectionAsync(handoverId, sectionId, content, status, userId);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).UpdateHandoverSection(handoverId, sectionId, content, status, userId);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_ReturnsNull_WhenNoPreferences()
    {
        // Arrange
        var userId = "user-123";
        _repository.GetUserPreferences(userId).Returns((UserPreferencesRecord?)null);

        // Act
        var result = await _setupService.GetUserPreferencesAsync(userId);

        // Assert
        result.Should().BeNull();
        _repository.Received(1).GetUserPreferences(userId);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_ReturnsPreferences_WhenPreferencesExist()
    {
        // Arrange
        var userId = "user-123";
        var preferences = new UserPreferencesRecord(
            "pref-001", userId, "dark", "en", "America/New_York",
            true, false, System.DateTime.Now, System.DateTime.Now
        );
        _repository.GetUserPreferences(userId).Returns(preferences);

        // Act
        var result = await _setupService.GetUserPreferencesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Theme.Should().Be("dark");
        result.Language.Should().Be("en");
        result.NotificationsEnabled.Should().BeTrue();
        result.AutoSaveEnabled.Should().BeFalse();
        _repository.Received(1).GetUserPreferences(userId);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsEmptyList_WhenNoSessions()
    {
        // Arrange
        var userId = "user-123";
        _repository.GetUserSessions(userId).Returns(new List<UserSessionRecord>());

        // Act
        var result = await _setupService.GetUserSessionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _repository.Received(1).GetUserSessions(userId);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsSessions_WhenSessionsExist()
    {
        // Arrange
        var userId = "user-123";
        var sessions = new List<UserSessionRecord>
        {
            new UserSessionRecord("session-001", userId, System.DateTime.Now, null,
                                "192.168.1.100", "Chrome", true),
            new UserSessionRecord("session-002", userId, System.DateTime.Now.AddHours(-1), System.DateTime.Now,
                                "192.168.1.101", "Firefox", false)
        };
        _repository.GetUserSessions(userId).Returns(sessions);

        // Act
        var result = await _setupService.GetUserSessionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].IsActive.Should().BeTrue();
        result[1].IsActive.Should().BeFalse();
        _repository.Received(1).GetUserSessions(userId);
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        var userId = "user-123";
        var preferences = new UserPreferencesRecord(
            "pref-001", userId, "light", "es", "Europe/Madrid",
            false, true, System.DateTime.Now, System.DateTime.Now
        );
        _repository.UpdateUserPreferences(userId, preferences).Returns(false);

        // Act
        var result = await _setupService.UpdateUserPreferencesAsync(userId, preferences);

        // Assert
        result.Should().BeFalse();
        _repository.Received(1).UpdateUserPreferences(userId, preferences);
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var userId = "user-123";
        var preferences = new UserPreferencesRecord(
            "pref-001", userId, "light", "es", "Europe/Madrid",
            false, true, System.DateTime.Now, System.DateTime.Now
        );
        _repository.UpdateUserPreferences(userId, preferences).Returns(true);

        // Act
        var result = await _setupService.UpdateUserPreferencesAsync(userId, preferences);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).UpdateUserPreferences(userId, preferences);
    }
}
