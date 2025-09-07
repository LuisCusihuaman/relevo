using Relevo.Core.ContributorAggregate;
using Xunit;
using System;
using Dapper;

namespace Relevo.IntegrationTests.Data;

public class ContributorServiceUpdate : BaseDapperTestFixture
{
  [Fact]
  public void UpdatesItemAfterAddingIt()
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

    // Update the item
    var newName = Guid.NewGuid().ToString();
    var updateSql = @"
      UPDATE Contributors
      SET Name = @Name
      WHERE Id = @Id";

    _connection.Execute(updateSql, new
    {
      Name = newName,
      Id = newId
    });

    // Fetch the updated item
    var selectSql = "SELECT Name, Status FROM Contributors WHERE Id = @Id";
    var updatedContributor = _connection.QueryFirstOrDefault<dynamic>(selectSql, new { Id = newId });

    Assert.NotNull(updatedContributor);
    Assert.Equal(newName, updatedContributor!.Name);
    Assert.Equal(0, updatedContributor.Status);
  }
}
