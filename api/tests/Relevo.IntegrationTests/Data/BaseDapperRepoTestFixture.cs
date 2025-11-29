using Microsoft.Extensions.DependencyInjection;
using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Relevo.Infrastructure.Data;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public abstract class BaseDapperRepoTestFixture : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly IServiceScope _scope;

    protected BaseDapperRepoTestFixture(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
    }

    protected ContributorRepository GetRepository()
    {
        return (ContributorRepository)_scope.ServiceProvider.GetRequiredService<IContributorRepository>();
    }
}
