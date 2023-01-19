namespace OsuDroid.Lib.TokenHandler;

public interface ITokenHandler : IRefreshAuto {
    public DateTime LastCleanTime { get; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Response<List<TokenInfoWithGuid>> GetAll();
    public void SetOverwriteMany(Span<TokenInfoWithGuid> span);
    public void CheckNow();
    public void RemoveAllTokenWithSameUserId(long userId);
    public void SetOverwrite(TokenInfoWithGuid tokenInfoWithGuid);
    public bool TokenExist(Guid token);

    public Guid Insert(long userId);

    public Response Refresh(Guid token);

    public void RemoveToken(Guid token);

    public Response<TokenInfo> GetTokenInfo(Guid token);

    public void RemoveDeadTokenIfNextCleanDate();

    public void RemoveDeadToken();
}