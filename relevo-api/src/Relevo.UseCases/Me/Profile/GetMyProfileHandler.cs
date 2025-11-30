using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Profile;

public class GetMyProfileHandler(IUserRepository _repository)
    : IRequestHandler<GetMyProfileQuery, Result<UserProfileRecord?>>
{
    public async Task<Result<UserProfileRecord?>> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetUserByIdAsync(request.UserId);
        return Result.Success(user);
    }
}

