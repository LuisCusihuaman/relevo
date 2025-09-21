using Relevo.Core.Interfaces;
using Relevo.UseCases.Setup;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Setup;

public class GetHandoverByIdUseCaseTests
{
    private readonly ISetupRepository _repository = Substitute.For<ISetupRepository>();
    private readonly GetHandoverByIdUseCase _useCase;

    public GetHandoverByIdUseCaseTests()
    {
        _useCase = new GetHandoverByIdUseCase(_repository);
    }

    [Fact]
    public void Execute_ReturnsHandover_WhenHandoverExists()
    {
        // Arrange
        var handoverId = "hvo-2509201329-4574";
        var expectedHandover = new HandoverRecord(
            Id: handoverId,
            AssignmentId: "assign-user_32GYA6PbtKI9GWMYIMebpoB59pS-shift-night-pat-026-20250920132921",
            PatientId: "pat-026",
            PatientName: "Álvaro Vargas",
            Status: "Active",
            CreatedAt: "2025-09-20 13:29:21",
            IllnessSeverity: new HandoverIllnessSeverity("Stable"),
            PatientSummary: new HandoverPatientSummary("Handover iniciado - información pendiente de completar"),
            ActionItems: new List<HandoverActionItem>(),
            SituationAwarenessDocId: null,
            Synthesis: null,
            ShiftName: "Noche",
            CreatedBy: "user_32GYA6PbtKI9GWMYIMebpoB59pS",
            AssignedTo: "user_32GYA6PbtKI9GWMYIMebpoB59pS"
        );

        _repository.GetHandoverById(handoverId).Returns(expectedHandover);

        // Act
        var result = _useCase.Execute(handoverId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedHandover);
        result!.Id.Should().Be(handoverId);
        result.PatientId.Should().Be("pat-026");
        result.Status.Should().Be("Active");
        result.PatientName.Should().Be("Álvaro Vargas");
        result.ShiftName.Should().Be("Noche");

        _repository.Received(1).GetHandoverById(handoverId);
    }

    [Fact]
    public void Execute_ReturnsNull_WhenHandoverDoesNotExist()
    {
        // Arrange
        var handoverId = "hvo-non-existent";

        _repository.GetHandoverById(handoverId).Returns((HandoverRecord?)null);

        // Act
        var result = _useCase.Execute(handoverId);

        // Assert
        result.Should().BeNull();

        _repository.Received(1).GetHandoverById(handoverId);
    }

    [Fact]
    public void Execute_ReturnsHandoverWithActionItems_WhenHandoverHasActionItems()
    {
        // Arrange
        var handoverId = "hvo-with-actions";
        var actionItems = new List<HandoverActionItem>
        {
            new HandoverActionItem("action-1", "Check vital signs", false),
            new HandoverActionItem("action-2", "Administer medication", true),
            new HandoverActionItem("action-3", "Update patient notes", false)
        };

        var expectedHandover = new HandoverRecord(
            Id: handoverId,
            AssignmentId: "assign-test",
            PatientId: "pat-test",
            PatientName: "Test Patient",
            Status: "InProgress",
            CreatedAt: "2025-09-20 14:00:00",
            IllnessSeverity: new HandoverIllnessSeverity("Watcher"),
            PatientSummary: new HandoverPatientSummary("Patient is stable but requires monitoring"),
            ActionItems: actionItems,
            SituationAwarenessDocId: "doc-123",
            Synthesis: new HandoverSynthesis("Patient condition has improved"),
            ShiftName: "Mañana",
            CreatedBy: "user-test",
            AssignedTo: "user-test"
        );

        _repository.GetHandoverById(handoverId).Returns(expectedHandover);

        // Act
        var result = _useCase.Execute(handoverId);

        // Assert
        result.Should().NotBeNull();
        result!.ActionItems.Should().NotBeNull();
        result.ActionItems.Should().HaveCount(3);
        result.ActionItems[0].Id.Should().Be("action-1");
        result.ActionItems[0].Description.Should().Be("Check vital signs");
        result.ActionItems[0].IsCompleted.Should().BeFalse();
        result.ActionItems[1].IsCompleted.Should().BeTrue();
        result.Synthesis.Should().NotBeNull();
        result.Synthesis!.Content.Should().Be("Patient condition has improved");

        _repository.Received(1).GetHandoverById(handoverId);
    }

    [Fact]
    public void Execute_ReturnsHandoverWithAllFieldsPopulated_WhenAllDataAvailable()
    {
        // Arrange
        var handoverId = "hvo-complete";
        var actionItems = new List<HandoverActionItem>
        {
            new HandoverActionItem("action-1", "Complete handover checklist", false)
        };

        var expectedHandover = new HandoverRecord(
            Id: handoverId,
            AssignmentId: "assign-complete-123",
            PatientId: "pat-complete-456",
            PatientName: "Complete Test Patient",
            Status: "Completed",
            CreatedAt: "2025-09-20 15:00:00",
            IllnessSeverity: new HandoverIllnessSeverity("Unstable"),
            PatientSummary: new HandoverPatientSummary("Patient showed signs of improvement"),
            ActionItems: actionItems,
            SituationAwarenessDocId: "situation-doc-789",
            Synthesis: new HandoverSynthesis("Handover completed successfully"),
            ShiftName: "Tarde",
            CreatedBy: "user-creator-123",
            AssignedTo: "user-assigned-456"
        );

        _repository.GetHandoverById(handoverId).Returns(expectedHandover);

        // Act
        var result = _useCase.Execute(handoverId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(handoverId);
        result.AssignmentId.Should().Be("assign-complete-123");
        result.PatientId.Should().Be("pat-complete-456");
        result.PatientName.Should().Be("Complete Test Patient");
        result.Status.Should().Be("Completed");
        result.IllnessSeverity.Severity.Should().Be("Unstable");
        result.PatientSummary.Content.Should().Be("Patient showed signs of improvement");
        result.ActionItems.Should().HaveCount(1);
        result.SituationAwarenessDocId.Should().Be("situation-doc-789");
        result.Synthesis.Should().NotBeNull();
        result.Synthesis!.Content.Should().Be("Handover completed successfully");
        result.ShiftName.Should().Be("Tarde");
        result.CreatedBy.Should().Be("user-creator-123");
        result.AssignedTo.Should().Be("user-assigned-456");

        _repository.Received(1).GetHandoverById(handoverId);
    }

    [Fact]
    public void Execute_HandlesDifferentStatusValuesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("hvo-active", "Active"),
            ("hvo-inprogress", "InProgress"),
            ("hvo-completed", "Completed")
        };

        foreach (var (handoverId, status) in testCases)
        {
            var expectedHandover = new HandoverRecord(
                Id: handoverId,
                AssignmentId: $"assign-{handoverId}",
                PatientId: $"pat-{handoverId}",
                PatientName: "Test Patient",
                Status: status,
                CreatedAt: "2025-09-20 16:00:00",
                IllnessSeverity: new HandoverIllnessSeverity("Stable"),
                PatientSummary: new HandoverPatientSummary("Test summary"),
                ActionItems: new List<HandoverActionItem>(),
                SituationAwarenessDocId: null,
                Synthesis: null,
                ShiftName: "Test Shift",
                CreatedBy: "test-user",
                AssignedTo: "test-user"
            );

            _repository.GetHandoverById(handoverId).Returns(expectedHandover);

            // Act
            var result = _useCase.Execute(handoverId);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(status);
        }
    }
}
