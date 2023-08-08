using Npgsql;

namespace OsuDroidAttachment.Interface;

public interface IDb : IAsyncDisposable {
    public ValueTask RollbackAsync();
    public ValueTask CommitAsync();
    public NpgsqlConnection Db { get; }
}