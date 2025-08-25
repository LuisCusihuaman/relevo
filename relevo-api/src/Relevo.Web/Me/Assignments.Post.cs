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
    string userId = "demo-user"; // placeholder until auth is wired
    _dataProvider.Assign(userId, req.ShiftId, req.PatientIds ?? []);
    await SendNoContentAsync(ct);
  }
}

public class PostAssignmentsRequest
{
  public string ShiftId { get; set; } = string.Empty;
  public List<string>? PatientIds { get; set; }
}


