using FastEndpoints;
using MediatR;
using Relevo.UseCases.Me.Assignments;

namespace Relevo.Web.Me;

public class PostAssignments(IMediator _mediator)
    : Endpoint<PostAssignmentsRequest>
{
    public override void Configure()
    {
        Post("/me/assignments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PostAssignmentsRequest req, CancellationToken ct)
    {
        // For now, use a hardcoded user ID (in production, this would come from auth context)
        var userId = "dr-1";

        var result = await _mediator.Send(
            new PostAssignmentsCommand(userId, req.ShiftId, req.PatientIds ?? []),
            ct);

        if (result.IsSuccess)
        {
            await SendNoContentAsync(ct);
        }
        else
        {
            await SendErrorsAsync(cancellation: ct);
        }
    }
}

public class PostAssignmentsRequest
{
    public string ShiftId { get; set; } = string.Empty;
    public List<string>? PatientIds { get; set; }
}

