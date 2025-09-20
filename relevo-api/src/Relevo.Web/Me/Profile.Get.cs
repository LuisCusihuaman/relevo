using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Me;

public class GetMyProfileEndpoint(
    IUserContext _userContext)
  : EndpointWithoutRequest<GetMyProfileResponse>
{
  public override void Configure()
  {
    Get("/me/profile");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    Response = new GetMyProfileResponse
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        Roles = user.Roles.ToList(),
        IsActive = user.IsActive
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetMyProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
