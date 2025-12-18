using FastEndpoints;
using Relevo.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Users;

public record GetAllUsersResponse
{
    public required IReadOnlyList<UserDto> users { get; init; }
}

public record UserDto
{
    [Required]
    public required string Id { get; init; }
    [Required]
    public required string Email { get; init; }
    [Required]
    public required string FirstName { get; init; }
    [Required]
    public required string LastName { get; init; }
    [Required]
    public required string FullName { get; init; }
}

public class GetAllUsersEndpoint(IUserRepository _userRepository)
    : EndpointWithoutRequest<GetAllUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _userRepository.GetAllUsersAsync();

        Response = new GetAllUsersResponse
        {
            users = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
            }).ToList()
        };
        await SendAsync(Response, cancellation: ct);
    }
}

