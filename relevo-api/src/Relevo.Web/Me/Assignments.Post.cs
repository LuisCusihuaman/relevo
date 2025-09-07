using FastEndpoints;

namespace Relevo.Web.Me;

public class PostAssignments(Relevo.Web.Setup.ISetupDataProvider _dataProvider)
  : Endpoint<PostAssignmentsRequest>
{
  public override void Configure()
  {
    Post("/me/assignments");
    AllowAnonymous();
  }

  public override async Task HandleAsync(PostAssignmentsRequest req, CancellationToken ct)
  {
    // TODO: Get actual user ID from authentication context when auth is implemented
    // For now, use a default user or get from request for testing
    string userId = req.UserId ?? "demo-user"; // Allow override via request for testing
    _dataProvider.Assign(userId, req.ShiftId, req.PatientIds ?? []);
    await SendNoContentAsync(ct);
  }
}

public class PostAssignmentsRequest
{
  public string? UserId { get; set; } // Optional override for testing
  public string ShiftId { get; set; } = string.Empty;
  public List<string>? PatientIds { get; set; }
}


