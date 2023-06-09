using Npgsql;

namespace OsuDroid.Lib.TokenHandler;

public interface ITokenHandlerDb : IRefreshAuto {
    public DateTime LastCleanTime { get; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Task<Result<List<TokenInfoWithGuid>, string>> GetAllAsync(NpgsqlConnection db);
    public Task<ResultErr<string>> SetOverwriteManyAsync(NpgsqlConnection db, IEnumerable<TokenInfoWithGuid> iter);
    public Task<ResultErr<string>> CheckNowAsync(NpgsqlConnection db);
    public Task<ResultErr<string>> RemoveAllTokenWithSameUserIdAsync(NpgsqlConnection db, long userId);
    public Task<ResultErr<string>> SetOverwriteAsync(NpgsqlConnection db, TokenInfoWithGuid tokenInfoWithGuid);
    public Task<Result<bool, string>> TokenExistAsync(NpgsqlConnection db, Guid token);

    public Task<Result<Guid, string>> InsertAsync(NpgsqlConnection db, long userId);

    public Task<ResultErr<string>> RefreshAsync(NpgsqlConnection db, Guid token);

    public Task<ResultErr<string>> RemoveTokenAsync(NpgsqlConnection db, Guid token);

    public Task<Result<Option<TokenInfo>, string>> GetTokenInfoAsync(NpgsqlConnection db, Guid token);

    public Task RemoveDeadTokenIfNextCleanDateAsync(NpgsqlConnection db);

    public Task<ResultErr<string>> RemoveDeadTokenAsync(NpgsqlConnection db);
}