using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.Create;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.Create;

public class CreateHandoverHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly CreateHandoverHandler _handler;

    public CreateHandoverHandlerHandle()
    {
        _handler = new CreateHandoverHandler(_repository, _userRepository);
    }

    [Fact]
    public async Task CreatesHandoverGivenValidRequest()
    {
        // V3: CreateHandoverAsync is refactored to use SHIFT_WINDOW_ID
        // It still creates handovers, but now uses V3 schema
        var command = new CreateHandoverCommand(
            "pat-1", "dr-1", "dr-2", "shift-1", "shift-2", "dr-1", "Notes");

        var createdHandover = new HandoverRecord(
            "hvo-new", // Id
            "pat-1", // PatientId
            "Test Patient", // PatientName
            "Draft", // Status
            "Stable", // IllnessSeverity
            "", // PatientSummary
            null, // SituationAwarenessDocId
            null, // Synthesis
            "Shift", // ShiftName
            "dr-1", // CreatedBy
            "dr-2", // AssignedTo
            null, // CreatedByName
            null, // AssignedToName
            null, // ReceiverUserId
            "dr-1", // ResponsiblePhysicianId
            "Dr. One", // ResponsiblePhysicianName
            null, // CreatedAt
            null, // ReadyAt
            null, // StartedAt
            null, // CompletedAt
            null, // CancelledAt
            null, // HandoverWindowDate
            "Draft", // StateName
            1, // Version
            "sw-1", // ShiftWindowId
            null, // PreviousHandoverId
            "dr-1", // SenderUserId
            null, // ReadyByUserId
            null, // StartedByUserId
            null, // CompletedByUserId
            null, // CancelledByUserId
            null); // CancelReason

        _repository.CreateHandoverAsync(Arg.Any<CreateHandoverRequest>())
            .Returns(Task.FromResult(createdHandover));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("hvo-new");
        
        // Verify EnsureUserExistsAsync was called
        await _userRepository.Received(1).EnsureUserExistsAsync(
            Arg.Is("dr-1"), 
            Arg.Any<string?>(), 
            Arg.Any<string?>(), 
            Arg.Any<string?>(), 
            Arg.Any<string?>(), 
            Arg.Any<string?>(), 
            Arg.Any<string?>());
    }
}

