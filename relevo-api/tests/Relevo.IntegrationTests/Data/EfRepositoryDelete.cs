using Relevo.Core.ContributorAggregate;
using Xunit;
using System;
using Dapper;

namespace Relevo.IntegrationTests.Data;

public class ContributorServiceDelete : BaseDapperTestFixture
{
  [Fact]
  public void DeletesItemAfterAddingIt()
  {
    // This test is skipped if Oracle is not available

    Assert.NotNull(_connection); // Additional safety check

    // Add a contributor
    var initialName = Guid.NewGuid().ToString();

    // Get next ID from sequence first
    var sequenceSql = "SELECT CONTRIBUTORS_SEQ.NEXTVAL FROM DUAL";
    var newId = _connection.ExecuteScalar<long>(sequenceSql, transaction: _transaction);

    var insertSql = @"
      INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL)
      VALUES (:Id, :Name, :Email)";

    _connection.Execute(insertSql, new
    {
      Id = newId,
      Name = initialName,
      Email = $"{initialName}@test.com"
    }, transaction: _transaction);

    // Delete the item
    var deleteSql = "DELETE FROM CONTRIBUTORS WHERE ID = :Id";
    _connection.Execute(deleteSql, new { Id = newId }, transaction: _transaction);

    // Verify it's no longer there
    var countSql = "SELECT COUNT(*) FROM CONTRIBUTORS WHERE ID = :Id";
    var count = _connection.ExecuteScalar<long>(countSql, new { Id = newId }, transaction: _transaction);

    Assert.Equal(0, count);
  }
}
