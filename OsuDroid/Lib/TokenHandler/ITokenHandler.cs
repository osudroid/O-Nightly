namespace OsuDroid.Lib.TokenHandler;

public interface ITokenHandler : IRefreshAuto {
    public DateTime LastCleanTime { get; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Result<List<TokenInfoWithGuid>, string> GetAll();
    public void SetOverwriteMany(Span<TokenInfoWithGuid> span);
    public void CheckNow();
    public void RemoveAllTokenWithSameUserId(long userId);
    public void SetOverwrite(TokenInfoWithGuid tokenInfoWithGuid);
    public bool TokenExist(Guid token);

    public Guid Insert(long userId);

    public ResultErr<string> Refresh(Guid token);

    public void RemoveToken(Guid token);

    public Option<TokenInfo> GetTokenInfo(Guid token);

    public void RemoveDeadTokenIfNextCleanDate();

    public void RemoveDeadToken();
}