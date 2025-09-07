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
    // Add a contributor
    var initialName = Guid.NewGuid().ToString();

    var insertSql = @"
      INSERT INTO Contributors (Name, Status)
      VALUES (@Name, @Status);
      SELECT last_insert_rowid();";

    var newId = _connection.ExecuteScalar<long>(insertSql, new
    {
      Name = initialName,
      Status = 0
    });

    // Delete the item
    var deleteSql = "DELETE FROM Contributors WHERE Id = @Id";
    _connection.Execute(deleteSql, new { Id = newId });

    // Verify it's no longer there
    var countSql = "SELECT COUNT(*) FROM Contributors WHERE Id = @Id";
    var count = _connection.ExecuteScalar<long>(countSql, new { Id = newId });

    Assert.Equal(0, count);
  }
}
