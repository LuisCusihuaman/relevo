using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Profile;

public record GetMyProfileQuery(string UserId) : IRequest<Result<UserProfileRecord?>>;

