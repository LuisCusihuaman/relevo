using Relevo.Core.Interfaces;
using Relevo.UseCases.Setup;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Setup;

public class GetPatientHandoversUseCaseTests
{
    private readonly ISetupRepository _repository = Substitute.For<ISetupRepository>();
    private readonly GetPatientHandoversUseCase _useCase;

    public GetPatientHandoversUseCaseTests()
    {
        _useCase = new GetPatientHandoversUseCase(_repository);
    }

    [Fact]
    public void Execute_ReturnsHandovers_WhenPatientHasHandovers()
    {
        // Arrange
        var patientId = "pat-001";
        var expectedHandovers = new List<HandoverRecord>
        {
            new HandoverRecord(
                Id: "hvo-001",
                AssignmentId: "assign-001",
                PatientId: patientId,
                PatientName: "Test Patient",
                Status: "Active",
                CreatedAt: "2025-09-20 10:00:00",
                IllnessSeverity: new HandoverIllnessSeverity("Stable"),
                PatientSummary: new HandoverPatientSummary("Patient summary"),
                ActionItems: new List<HandoverActionItem>(),
                SituationAwarenessDocId: "doc-001",
                Synthesis: null,
                ShiftName: "Mañana",
                CreatedBy: "user-123",
                AssignedTo: "user-123"
            )
        };

        _repository.GetPatientHandovers(patientId, 1, 25)
            .Returns((expectedHandovers, 1));

        // Act
        var (handovers, totalCount) = _useCase.Execute(patientId, 1, 25);

        // Assert
        handovers.Should().NotBeNull();
        handovers.Should().HaveCount(1);
        handovers[0].PatientId.Should().Be(patientId);
        handovers[0].Status.Should().Be("Active");
        totalCount.Should().Be(1);

        _repository.Received(1).GetPatientHandovers(patientId, 1, 25);
    }

    [Fact]
    public void Execute_ReturnsEmptyList_WhenPatientHasNoHandovers()
    {
        // Arrange
        var patientId = "pat-999";

        _repository.GetPatientHandovers(patientId, 1, 25)
            .Returns((new List<HandoverRecord>(), 0));

        // Act
        var (handovers, totalCount) = _useCase.Execute(patientId, 1, 25);

        // Assert
        handovers.Should().NotBeNull();
        handovers.Should().BeEmpty();
        totalCount.Should().Be(0);

        _repository.Received(1).GetPatientHandovers(patientId, 1, 25);
    }

    [Fact]
    public void Execute_HandlesPaginationCorrectly()
    {
        // Arrange
        var patientId = "pat-001";
        var page = 2;
        var pageSize = 10;

        _repository.GetPatientHandovers(patientId, page, pageSize)
            .Returns((new List<HandoverRecord>(), 25));

        // Act
        var (handovers, totalCount) = _useCase.Execute(patientId, page, pageSize);

        // Assert
        handovers.Should().NotBeNull();
        totalCount.Should().Be(25);

        _repository.Received(1).GetPatientHandovers(patientId, page, pageSize);
    }

    [Fact]
    public void Execute_ReturnsMultipleHandovers_WithCorrectOrder()
    {
        // Arrange
        var patientId = "pat-001";
        var expectedHandovers = new List<HandoverRecord>
        {
            new HandoverRecord(
                Id: "hvo-002",
                AssignmentId: "assign-002",
                PatientId: patientId,
                PatientName: "Test Patient",
                Status: "Completed",
                CreatedAt: "2025-09-20 11:00:00",
                IllnessSeverity: new HandoverIllnessSeverity("Stable"),
                PatientSummary: new HandoverPatientSummary("Patient summary 2"),
                ActionItems: new List<HandoverActionItem>(),
                SituationAwarenessDocId: "doc-002",
                Synthesis: new HandoverSynthesis("Synthesis 2"),
                ShiftName: "Noche",
                CreatedBy: "user-123",
                AssignedTo: "user-123"
            ),
            new HandoverRecord(
                Id: "hvo-001",
                AssignmentId: "assign-001",
                PatientId: patientId,
                PatientName: "Test Patient",
                Status: "Active",
                CreatedAt: "2025-09-20 12:00:00",
                IllnessSeverity: new HandoverIllnessSeverity("Watcher"),
                PatientSummary: new HandoverPatientSummary("Patient summary 1"),
                ActionItems: new List<HandoverActionItem>(),
                SituationAwarenessDocId: "doc-001",
                Synthesis: null,
                ShiftName: "Mañana",
                CreatedBy: "user-123",
                AssignedTo: "user-123"
            )
        };

        _repository.GetPatientHandovers(patientId, 1, 25)
            .Returns((expectedHandovers, 2));

        // Act
        var (handovers, totalCount) = _useCase.Execute(patientId, 1, 25);

        // Assert
        handovers.Should().NotBeNull();
        handovers.Should().HaveCount(2);
        handovers[0].Id.Should().Be("hvo-002");
        handovers[1].Id.Should().Be("hvo-001");
        totalCount.Should().Be(2);
    }
}
