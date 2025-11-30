using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Assignments;

namespace Relevo.Web.Me;

public class PostAssignments(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<PostAssignmentsRequest>
{
    public override void Configure()
    {
        Post("/me/assignments");
    }

    public override async Task HandleAsync(PostAssignmentsRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
             await SendUnauthorizedAsync(ct);
             return;
        }

        try
        {
            var result = await _mediator.Send(
                new PostAssignmentsCommand(
                    userId, 
                    req.ShiftId, 
                    req.PatientIds ?? [], 
                    _currentUser.Email,
                    _currentUser.FirstName,
                    _currentUser.LastName,
                    _currentUser.FullName,
                    _currentUser.AvatarUrl,
                    _currentUser.OrgRole
                ),
                ct);

            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                AddError("Failed to assign patients");
                await SendErrorsAsync(statusCode: 400, ct);
            }
        }
        catch (Exception)
        {
            AddError("Assignment failed: referenced shift or patient does not exist");
            await SendErrorsAsync(statusCode: 400, ct);
        }
    }
}

public class PostAssignmentsRequest
{
    public string ShiftId { get; set; } = string.Empty;
    public List<string>? PatientIds { get; set; }
}

