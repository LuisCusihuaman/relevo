using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Profile;

namespace Relevo.Web.Me;

public record GetMyProfileResponse
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
    public bool IsActive { get; init; } = true;
}

public class GetMyProfileEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : EndpointWithoutRequest<GetMyProfileResponse>
{
    public override void Configure()
    {
        Get("/me/profile");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await _mediator.Send(new GetMyProfileQuery(userId), ct);

        if (!result.IsSuccess || result.Value == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var user = result.Value;

        Response = new GetMyProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = user.Roles,
            IsActive = user.IsActive
        };
        await SendAsync(Response, cancellation: ct);
    }
}

