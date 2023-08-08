using LamLogger;
using Npgsql;

namespace OsuDroid.Lib.Handler;

internal record ConStart
    (NpgsqlConnection DbNormal, NpgsqlTransaction DbTransaction, LamLog LamLog) : IAsyncDisposable, IDisposable {
    public async ValueTask DisposeAsync() {
        LamLog.Dispose();
        await DbTransaction.DisposeAsync();
        await DbNormal.DisposeAsync();
    }

    public void Dispose() {
        DisposeAsync().AsTask().Wait();
    }

    public (NpgsqlTransaction dbT, NpgsqlConnection db, LamLog Log) Unpack() {
        return (DbTransaction, DbTransaction.Connection!, LamLog);
    }
}