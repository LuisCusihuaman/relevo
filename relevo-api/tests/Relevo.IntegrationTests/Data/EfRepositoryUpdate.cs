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

    // Update the item
    var newName = Guid.NewGuid().ToString();
    var updateSql = @"
      UPDATE CONTRIBUTORS
      SET NAME = :Name
      WHERE ID = :Id";

    _connection.Execute(updateSql, new
    {
      Name = newName,
      Id = newId
    }, transaction: _transaction);

    // Fetch the updated item
    var selectSql = "SELECT NAME, EMAIL FROM CONTRIBUTORS WHERE ID = :Id";
    var updatedContributor = _connection.QueryFirstOrDefault<dynamic>(selectSql, new { Id = newId }, transaction: _transaction);

    Assert.NotNull(updatedContributor);
    Assert.Equal(newName, updatedContributor!.NAME);
    Assert.Equal($"{initialName}@test.com", updatedContributor.EMAIL);
  }
}
