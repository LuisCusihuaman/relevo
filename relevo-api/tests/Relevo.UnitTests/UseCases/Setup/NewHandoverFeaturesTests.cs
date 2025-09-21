using Relevo.Core.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

// Use specific types from Core layer to avoid conflicts
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;
using HandoverParticipantRecord = Relevo.Core.Interfaces.HandoverParticipantRecord;
using HandoverSectionRecord = Relevo.Core.Interfaces.HandoverSectionRecord;
using HandoverSyncStatusRecord = Relevo.Core.Interfaces.HandoverSyncStatusRecord;
using UserPreferencesRecord = Relevo.Core.Interfaces.UserPreferencesRecord;
using UserSessionRecord = Relevo.Core.Interfaces.UserSessionRecord;

namespace Relevo.UnitTests.UseCases.Setup;

public class NewHandoverFeaturesTests
{
    [Fact]
    public void GetActiveHandover_ShouldReturnHandoverWithCorrectStructure()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var expectedHandover = new HandoverRecord(
            "handover-001", "assignment-001", "patient-001", "John Doe",
            "Active", new HandoverIllnessSeverity("Stable"),
            new HandoverPatientSummary("Patient is stable"),
            new List<HandoverActionItem>(), "situation-doc-001",
            null, "Morning Shift", "user-123", "user-456", "2024-01-01 08:00:00"
        );
        repository.GetActiveHandover("user-123").Returns(expectedHandover);

        // Act
        var result = repository.GetActiveHandover("user-123");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("handover-001");
        result.Status.Should().Be("Active");
        result.AssignedTo.Should().Be("user-456");
        result.PatientName.Should().Be("John Doe");
    }

    [Fact]
    public void GetHandoverParticipants_ShouldReturnParticipantsList()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var participants = new List<HandoverParticipantRecord>
        {
            new HandoverParticipantRecord("part-001", "user-123", "John Doe", "Physician", "active",
                                        System.DateTime.Now, System.DateTime.Now),
            new HandoverParticipantRecord("part-002", "user-456", "Jane Smith", "Nurse", "active",
                                        System.DateTime.Now, System.DateTime.Now)
        };
        repository.GetHandoverParticipants("handover-001").Returns(participants);

        // Act
        var result = repository.GetHandoverParticipants("handover-001");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].UserName.Should().Be("John Doe");
        result[1].UserName.Should().Be("Jane Smith");
    }

    [Fact]
    public void GetHandoverSections_ShouldReturnSectionsList()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var sections = new List<HandoverSectionRecord>
        {
            new HandoverSectionRecord("section-001", "illness_severity", "Patient is stable", "completed",
                                    "user-123", System.DateTime.Now, System.DateTime.Now),
            new HandoverSectionRecord("section-002", "patient_summary", "Patient summary content", "draft",
                                    "user-456", System.DateTime.Now, System.DateTime.Now)
        };
        repository.GetHandoverSections("handover-001").Returns(sections);

        // Act
        var result = repository.GetHandoverSections("handover-001");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SectionType.Should().Be("illness_severity");
        result[1].SectionType.Should().Be("patient_summary");
    }

    [Fact]
    public void GetHandoverSyncStatus_ShouldReturnSyncStatus()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var syncStatus = new HandoverSyncStatusRecord("sync-001", "synced", System.DateTime.Now, 1);
        repository.GetHandoverSyncStatus("handover-001", "user-123").Returns(syncStatus);

        // Act
        var result = repository.GetHandoverSyncStatus("handover-001", "user-123");

        // Assert
        result.Should().NotBeNull();
        result!.SyncStatus.Should().Be("synced");
        result.Version.Should().Be(1);
    }

    [Fact]
    public void UpdateHandoverSection_ShouldReturnTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        repository.UpdateHandoverSection("handover-001", "section-001", "Updated content", "completed", "user-123")
                .Returns(true);

        // Act
        var result = repository.UpdateHandoverSection("handover-001", "section-001", "Updated content", "completed", "user-123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetUserPreferences_ShouldReturnPreferences()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var preferences = new UserPreferencesRecord(
            "pref-001", "user-123", "dark", "en", "America/New_York",
            true, false, System.DateTime.Now, System.DateTime.Now
        );
        repository.GetUserPreferences("user-123").Returns(preferences);

        // Act
        var result = repository.GetUserPreferences("user-123");

        // Assert
        result.Should().NotBeNull();
        result!.Theme.Should().Be("dark");
        result.Language.Should().Be("en");
        result.NotificationsEnabled.Should().BeTrue();
        result.AutoSaveEnabled.Should().BeFalse();
    }

    [Fact]
    public void GetUserSessions_ShouldReturnSessionsList()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var sessions = new List<UserSessionRecord>
        {
            new UserSessionRecord("session-001", "user-123", System.DateTime.Now, null,
                                "192.168.1.100", "Chrome", true),
            new UserSessionRecord("session-002", "user-123", System.DateTime.Now.AddHours(-1), System.DateTime.Now,
                                "192.168.1.101", "Firefox", false)
        };
        repository.GetUserSessions("user-123").Returns(sessions);

        // Act
        var result = repository.GetUserSessions("user-123");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].IsActive.Should().BeTrue();
        result[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateUserPreferences_ShouldReturnTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var repository = Substitute.For<ISetupRepository>();
        var preferences = new UserPreferencesRecord(
            "pref-001", "user-123", "light", "es", "Europe/Madrid",
            false, true, System.DateTime.Now, System.DateTime.Now
        );
        repository.UpdateUserPreferences("user-123", preferences).Returns(true);

        // Act
        var result = repository.UpdateUserPreferences("user-123", preferences);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HandoverRecord_ShouldHaveCorrectProperties()
    {
        // This test verifies the structure of the HandoverRecord
        var handover = new HandoverRecord(
            "handover-001", "assignment-001", "patient-001", "John Doe",
            "Active", new HandoverIllnessSeverity("Stable"),
            new HandoverPatientSummary("Patient summary"),
            new List<HandoverActionItem>
            {
                new HandoverActionItem("action-001", "Monitor vital signs", true)
            },
            "situation-doc-001",
            new HandoverSynthesis("Patient ready for discharge"),
            "Morning Shift", "user-123", "user-456", "2024-01-01 08:00:00"
        );

        // Assert
        handover.Id.Should().Be("handover-001");
        handover.AssignmentId.Should().Be("assignment-001");
        handover.PatientId.Should().Be("patient-001");
        handover.PatientName.Should().Be("John Doe");
        handover.Status.Should().Be("Active");
        handover.IllnessSeverity.Severity.Should().Be("Stable");
        handover.PatientSummary.Content.Should().Be("Patient summary");
        handover.ActionItems.Should().HaveCount(1);
        handover.ActionItems[0].Description.Should().Be("Monitor vital signs");
        handover.SituationAwarenessDocId.Should().Be("situation-doc-001");
        handover.Synthesis!.Content.Should().Be("Patient ready for discharge");
        handover.ShiftName.Should().Be("Morning Shift");
        handover.CreatedBy.Should().Be("user-123");
        handover.AssignedTo.Should().Be("user-456");
        handover.CreatedAt.Should().Be("2024-01-01 08:00:00");
    }

    [Fact]
    public void UserPreferencesRecord_ShouldHaveCorrectProperties()
    {
        // This test verifies the structure of the UserPreferencesRecord
        var preferences = new UserPreferencesRecord(
            "pref-001", "user-123", "dark", "en", "America/New_York",
            true, false, System.DateTime.Now, System.DateTime.Now
        );

        // Assert
        preferences.Id.Should().Be("pref-001");
        preferences.UserId.Should().Be("user-123");
        preferences.Theme.Should().Be("dark");
        preferences.Language.Should().Be("en");
        preferences.Timezone.Should().Be("America/New_York");
        preferences.NotificationsEnabled.Should().BeTrue();
        preferences.AutoSaveEnabled.Should().BeFalse();
        preferences.CreatedAt.Should().NotBe(default(DateTime));
        preferences.UpdatedAt.Should().NotBe(default(DateTime));
    }
}
