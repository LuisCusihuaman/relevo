using Relevo.Core.ContributorAggregate;
using Xunit;
using Dapper;

namespace Relevo.IntegrationTests.Data;

public class ContributorServiceAdd : BaseDapperTestFixture
{
  [Fact]
  public void AddsContributorAndSetsId()
  {
    // This test is skipped if Oracle is not available

    Assert.NotNull(_connection); // Additional safety check

    // Test basic database operations with Oracle syntax
    var testContributorName = "testContributor";

    // Get next ID from sequence first
    var sequenceSql = "SELECT CONTRIBUTORS_SEQ.NEXTVAL FROM DUAL";
    var newId = _connection.ExecuteScalar<long>(sequenceSql, transaction: _transaction);

    // Insert contributor with the new ID
    var insertSql = @"
      INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL)
      VALUES (:Id, :Name, :Email)";

    var insertParams = new
    {
      Id = newId,
      Name = testContributorName,
      Email = $"{testContributorName}@test.com"
    };

    _connection.Execute(insertSql, insertParams, transaction: _transaction);

    // Verify the contributor was added
    var selectSql = "SELECT NAME, EMAIL FROM CONTRIBUTORS WHERE ID = :Id";
    var contributor = _connection.QueryFirstOrDefault<dynamic>(selectSql, new { Id = newId }, transaction: _transaction);

    Assert.NotNull(contributor);
    Assert.Equal(testContributorName, contributor!.NAME);
    Assert.Equal($"{testContributorName}@test.com", contributor.EMAIL);
    Assert.True(newId > 0);
  }
}
