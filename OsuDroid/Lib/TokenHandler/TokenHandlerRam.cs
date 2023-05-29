using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OsuDroid.Lib.TokenHandler;

public class TokenHandlerRam : ITokenHandler {
    private readonly ConcurrentDictionary<Guid, TokenInfo> TokenDictionary = new();

    public TokenHandlerRam(DateTime lastCleanTime, TimeSpan cleanInterval, TimeSpan lifeSpanToken) {
        LastCleanTime = lastCleanTime;
        CleanInterval = cleanInterval;
        LifeSpanToken = lifeSpanToken;
    }

    public DateTime LastCleanTime { get; private set; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Result<List<TokenInfoWithGuid>, string> GetAll() {
        throw new NotImplementedException();
    }

    public void SetOverwrite(TokenInfoWithGuid tokenInfoWithGuid) {
        TokenDictionary
            .AddOrUpdate(
                tokenInfoWithGuid.Token,
                _ => tokenInfoWithGuid.TokenInfo,
                (_, _) => tokenInfoWithGuid.TokenInfo);
    }

    public void SetOverwriteMany(Span<TokenInfoWithGuid> span) {
        ref var searchSpace = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < span.Length; i++) {
            var iteam = Unsafe.Add(ref searchSpace, i);
            SetOverwrite(iteam);
        }
    }

    public void CheckNow() {
        LastCleanTime = DateTime.UtcNow;
        RemoveDeadToken();
    }

    public void RemoveAllTokenWithSameUserId(long userId) {
        foreach (var (key, value) in TokenDictionary.Where(x => x.Value.UserId == userId))
            TokenDictionary.TryRemove(key, out var _);
    }

    public bool TokenExist(Guid token) {
        if (TokenDictionary.TryGetValue(token, out var tokenInfo) == false)
            return false;
        return CheckLifeTime(ref tokenInfo);
    }

    public Guid Insert(long userId) {
        var guid = Guid.NewGuid();
        TokenDictionary.TryAdd(guid, new TokenInfo { CreateDay = DateTime.UtcNow, UserId = userId });
        return guid;
    }

    public ResultErr<string> Refresh(Guid token) {
        if (TokenDictionary.TryGetValue(token, out var tokenInfo) == false)
            return ResultErr<string>.Err("Can Refresh token, token Not Found");
        SetOverwrite(new TokenInfoWithGuid {
            TokenInfo = new TokenInfo { CreateDay = DateTime.UtcNow, UserId = tokenInfo.UserId },
            Token = token
        });

        return ResultErr<string>.Ok();
    }

    public void RemoveToken(Guid token) {
        TokenDictionary.TryRemove(token, out var _);
    }

    public Option<TokenInfo> GetTokenInfo(Guid token) {
        if (TokenDictionary.TryGetValue(token, out var tokenInfo) == false) 
            return Option<TokenInfo>.Empty;

        if (CheckLifeTime(ref tokenInfo) == false) {
            TokenDictionary.TryRemove(token, out _);
            return Option<TokenInfo>.Empty;
        }

        return Option<TokenInfo>.With(tokenInfo);
    }

    public void RemoveDeadTokenIfNextCleanDate() {
        if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
        LastCleanTime = DateTime.UtcNow;
        Task.Factory.StartNew(RemoveDeadToken);
    }

    public void RefreshAuto() {
        RemoveDeadTokenIfNextCleanDate();
    }

    public void RemoveDeadToken() {
        var now = DateTime.UtcNow;
        foreach (var (key, value) in TokenDictionary) {
            if (value.CreateDay.Add(LifeSpanToken) > now) continue;
            TokenDictionary.TryRemove(key, out var _);
        }
    }


    private bool CheckLifeTime(ref TokenInfo tokenInfo) {
        return tokenInfo.CreateDay.Add(LifeSpanToken) > DateTime.UtcNow;
    }

    private bool CheckLifeTime(DateTime now, ref TokenInfo tokenInfo) {
        return tokenInfo.CreateDay.Add(LifeSpanToken) > now;
    }
}