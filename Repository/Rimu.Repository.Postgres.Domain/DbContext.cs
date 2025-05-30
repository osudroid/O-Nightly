using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Domain;

public class DbContext: IDbContext {
    private NpgsqlConnection? _connection;
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Retrieves the current database connection, creating a new one if necessary.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The database connection.</returns>
    public ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default) {
        if (cancellationToken.IsCancellationRequested) {
            return ValueTask.FromCanceled<NpgsqlConnection>(cancellationToken);
        }
        
        return _connection is null 
            ? CreateAndSetConnection(cancellationToken) 
            : ValueTask.FromResult(_connection!);
    }

    /// <summary>
    /// Creates and sets a new database connection.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The new database connection.</returns>
    private async ValueTask<NpgsqlConnection> CreateAndSetConnection(CancellationToken cancellationToken) {
        _connection = await NpgsqlBuilder.BuildAsync(cancellationToken);
        return _connection;
    }

    ~DbContext() {
        Dispose();
    }
    
    public ValueTask DisposeAsync() {
        if (IsDisposed) {
            return ValueTask.CompletedTask;
        }
        
        GC.SuppressFinalize(this);
        
        return _connection?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    public void Dispose() {
        DisposeAsync().GetAwaiter().GetResult();
    }
}