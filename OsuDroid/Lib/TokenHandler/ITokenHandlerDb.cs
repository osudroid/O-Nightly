namespace OsuDroid.Lib.TokenHandler;

public interface ITokenHandlerDb : IRefreshAuto {
    public DateTime LastCleanTime { get; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Result<List<TokenInfoWithGuid>, string> GetAll(SavePoco db);
    public ResultErr<string> SetOverwriteMany(SavePoco db, Span<TokenInfoWithGuid> span);
    public ResultErr<string> CheckNow(SavePoco db);
    public ResultErr<string> RemoveAllTokenWithSameUserId(SavePoco db, long userId);
    public ResultErr<string> SetOverwrite(SavePoco db, TokenInfoWithGuid tokenInfoWithGuid);
    public Result<bool, string> TokenExist(SavePoco db, Guid token);

    public Result<Guid, string> Insert(SavePoco db, long userId);

    public ResultErr<string> Refresh(SavePoco db, Guid token);

    public ResultErr<string> RemoveToken(SavePoco db, Guid token);

    public Result<Option<TokenInfo>, string> GetTokenInfo(SavePoco db, Guid token);

    public void RemoveDeadTokenIfNextCleanDate(SavePoco db);

    public ResultErr<string> RemoveDeadToken(SavePoco db);
}