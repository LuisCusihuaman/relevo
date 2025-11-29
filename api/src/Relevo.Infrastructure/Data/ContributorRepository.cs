using System.Data;
using Ardalis.SharedKernel;
using Dapper;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;

namespace Relevo.Infrastructure.Data;

file sealed record ContributorRow(
  long Id,
  string Name,
  ContributorStatus Status,
  string? PhoneNumber_CountryCode,
  string? PhoneNumber_Number,
  string? PhoneNumber_Extension
);

public class ContributorRepository(DapperConnectionFactory _connectionFactory, IDomainEventDispatcher? _dispatcher) : IContributorRepository
{
  public async Task<Contributor?> GetByIdAsync(int id, CancellationToken ct = default)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = """
      SELECT
        Id,
        Name,
        Status,
        PhoneNumber_CountryCode,
        PhoneNumber_Number,
        PhoneNumber_Extension
      FROM Contributors
      WHERE Id = :Id
    """;

    var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
    var row = await conn.QuerySingleOrDefaultAsync<ContributorRow>(command);
    
    if (row is null) return null;

    var contributor = new Contributor(row.Name);
    
    PhoneNumber? phone = row.PhoneNumber_Number is null
      ? null
      : new PhoneNumber(row.PhoneNumber_CountryCode ?? "", row.PhoneNumber_Number, row.PhoneNumber_Extension);

    contributor.Rehydrate((int)row.Id, row.Status, phone);
    
    return contributor;
  }

  public async Task<Contributor> AddAsync(Contributor entity, CancellationToken ct = default)
  {
    using var conn = _connectionFactory.CreateConnection();

    // Get ID first to avoid RETURNING clause issues with Dapper/Oracle
    var newIdLong = await conn.ExecuteScalarAsync<long>("SELECT CONTRIBUTORS_SEQ.NEXTVAL FROM DUAL");
    var newId = (int)newIdLong;

    const string sql = """
      INSERT INTO Contributors
        (Id, Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension)
      VALUES
        (:Id, :Name, :Status, :CountryCode, :PhoneNum, :Extension)
    """;

    var parameters = new
    {
      Id = newId,
      Name = entity.Name,
      Status = entity.Status,
      CountryCode = entity.PhoneNumber?.CountryCode,
      PhoneNum = entity.PhoneNumber?.Number,
      Extension = entity.PhoneNumber?.Extension
    };

    var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
    await conn.ExecuteAsync(command);

    entity.Rehydrate(newId, entity.Status, entity.PhoneNumber);

    await DispatchEvents(entity);
    return entity;
  }

  public async Task UpdateAsync(Contributor entity, CancellationToken ct = default)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = """
      UPDATE Contributors
      SET 
        Name = :Name,
        Status = :Status,
        PhoneNumber_CountryCode = :CountryCode,
        PhoneNumber_Number = :PhoneNum,
        PhoneNumber_Extension = :Extension
      WHERE Id = :Id
    """;

    var command = new CommandDefinition(sql, new
    {
      entity.Name,
      Status = entity.Status,
      CountryCode = entity.PhoneNumber?.CountryCode,
      PhoneNum = entity.PhoneNumber?.Number,
      Extension = entity.PhoneNumber?.Extension,
      entity.Id
    }, cancellationToken: ct);

    await conn.ExecuteAsync(command);

    await DispatchEvents(entity);
  }

  public async Task DeleteAsync(Contributor entity, CancellationToken ct = default)
  {
    using var conn = _connectionFactory.CreateConnection();
    var command = new CommandDefinition("DELETE FROM Contributors WHERE Id = :Id", new { entity.Id }, cancellationToken: ct);
    await conn.ExecuteAsync(command);
    await DispatchEvents(entity);
  }

  private async Task DispatchEvents(EntityBase entity)
  {
    if (_dispatcher != null && entity.DomainEvents.Any())
    {
      await _dispatcher.DispatchAndClearEvents(new[] { entity });
    }
  }
}
