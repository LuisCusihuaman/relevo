using Relevo.Core.ContributorAggregate;
using Relevo.FunctionalTests;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class ContributorRepositoryTests : BaseDapperRepoTestFixture
{
    public ContributorRepositoryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddAndGetContributor()
    {
        var repository = GetRepository();
        var name = "IntegrationTest_" + Guid.NewGuid();
        var contributor = new Contributor(name);

        // Add
        var added = await repository.AddAsync(contributor);
        Assert.True(added.Id > 0);
        Assert.Equal(name, added.Name);

        // Get
        var fetched = await repository.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(added.Id, fetched.Id);
        Assert.Equal(name, fetched.Name);
    }

    [Fact]
    public async Task UpdateContributor()
    {
        var repository = GetRepository();
        var name = "UpdateTest_" + Guid.NewGuid();
        var contributor = new Contributor(name);
        await repository.AddAsync(contributor);

        var newName = name + "_Updated";
        contributor.UpdateName(newName);
        await repository.UpdateAsync(contributor);

        var fetched = await repository.GetByIdAsync(contributor.Id);
        Assert.NotNull(fetched);
        Assert.Equal(newName, fetched.Name);
    }

    [Fact]
    public async Task DeleteContributor()
    {
        var repository = GetRepository();
        var name = "DeleteTest_" + Guid.NewGuid();
        var contributor = new Contributor(name);
        await repository.AddAsync(contributor);

        await repository.DeleteAsync(contributor);

        var fetched = await repository.GetByIdAsync(contributor.Id);
        Assert.Null(fetched);
    }
}
