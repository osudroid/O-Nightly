using LamLogger;
using Mediator;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.View;

namespace OsuDroid.Lib.Handler; 

record ConStart
    (NpgsqlConnection DbNormal, NpgsqlTransaction DbTransaction, LamLog LamLog) : IAsyncDisposable, IDisposable {
    public async ValueTask DisposeAsync() {
        LamLog.Dispose();
        await DbTransaction.DisposeAsync();
        await DbNormal.DisposeAsync();
    }

    public (NpgsqlTransaction dbT, NpgsqlConnection db, LamLog Log) Unpack() =>
        (DbTransaction, DbTransaction.Connection!, LamLog);

    public void Dispose() {
        DisposeAsync().AsTask().Wait();
    }
}