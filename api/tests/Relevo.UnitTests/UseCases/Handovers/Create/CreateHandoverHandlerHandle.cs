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
        var command = new CreateHandoverCommand(
            "pat-1", "dr-1", "dr-2", "shift-1", "shift-2", "dr-1", "Notes");

        var createdHandover = new HandoverRecord("hvo-new", "asn-new", "pat-1", "Test Patient", "Draft",
            new HandoverIllnessSeverity("Stable"), new HandoverPatientSummary(""), null, null,
            "Shift", "dr-1", "dr-2", null, null, null, "dr-1", "Dr. One",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Draft", 1);

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

