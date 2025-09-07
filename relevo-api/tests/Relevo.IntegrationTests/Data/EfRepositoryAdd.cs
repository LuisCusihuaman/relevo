using Relevo.Core.ContributorAggregate;
using Xunit;
using Dapper;

namespace Relevo.IntegrationTests.Data;

public class ContributorServiceAdd : BaseDapperTestFixture
{
  [Fact]
  public void AddsContributorAndSetsId()
  {
    // Test basic database operations with Dapper directly
    var testContributorName = "testContributor";

    var sql = @"
      INSERT INTO Contributors (Name, Status)
      VALUES (@Name, @Status);
      SELECT last_insert_rowid();";

    var newId = _connection.ExecuteScalar<long>(sql, new
    {
      Name = testContributorName,
      Status = 0 // NotSet = 0
    });

    // Verify the contributor was added
    var selectSql = "SELECT Name, Status FROM Contributors WHERE Id = @Id";
    var contributor = _connection.QueryFirstOrDefault<dynamic>(selectSql, new { Id = newId });

    Assert.NotNull(contributor);
    Assert.Equal(testContributorName, contributor!.Name);
    Assert.Equal(0, contributor.Status);
    Assert.True(newId > 0);
  }
}
