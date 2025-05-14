using Npgsql;

namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IDbContext: IAsyncDisposable, IDisposable {
    public ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}