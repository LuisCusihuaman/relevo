using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Me;

public class PostAssignments(
    Relevo.Web.Setup.ISetupDataProvider _dataProvider,
    IUserContext _userContext)
  : Endpoint<PostAssignmentsRequest>
{
  public override void Configure()
  {
    Post("/me/assignments");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(PostAssignmentsRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _dataProvider.Assign(user.Id, req.ShiftId, req.PatientIds ?? []);
    await SendNoContentAsync(ct);
  }
}

public class PostAssignmentsRequest
{
  public string ShiftId { get; set; } = string.Empty;
  public List<string>? PatientIds { get; set; }
}


