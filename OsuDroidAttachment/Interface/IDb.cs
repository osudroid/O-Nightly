using Npgsql;

namespace OsuDroidAttachment.Interface;

public interface IDb : IAsyncDisposable {
    public NpgsqlConnection Db { get; }
    public ValueTask RollbackAsync();
    public ValueTask CommitAsync();
}