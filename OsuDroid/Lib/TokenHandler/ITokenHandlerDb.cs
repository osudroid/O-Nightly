namespace OsuDroid.Lib.TokenHandler;

public interface ITokenHandlerDb : IRefreshAuto {
    public DateTime LastCleanTime { get; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Response<List<TokenInfoWithGuid>> GetAll(SavePoco db);
    public void SetOverwriteMany(SavePoco db, Span<TokenInfoWithGuid> span);
    public void CheckNow(SavePoco db);
    public void RemoveAllTokenWithSameUserId(SavePoco db, long userId);
    public void SetOverwrite(SavePoco db, TokenInfoWithGuid tokenInfoWithGuid);
    public bool TokenExist(SavePoco db, Guid token);

    public Guid Insert(SavePoco db, long userId);

    public Response Refresh(SavePoco db, Guid token);

    public void RemoveToken(SavePoco db, Guid token);

    public Response<TokenInfo> GetTokenInfo(SavePoco db, Guid token);

    public void RemoveDeadTokenIfNextCleanDate(SavePoco db);

    public void RemoveDeadToken(SavePoco db);
}