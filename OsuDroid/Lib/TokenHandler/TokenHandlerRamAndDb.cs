using System.Runtime.InteropServices;

namespace OsuDroid.Lib.TokenHandler;

public class TokenHandlerRamAndDb : ITokenHandlerDb {
    private readonly ITokenHandlerDb _db;
    private readonly ITokenHandler _ram;

    public TokenHandlerRamAndDb(DateTime lastCleanTime, TimeSpan cleanInterval, TimeSpan lifeSpanToken) {
        LastCleanTime = lastCleanTime;
        CleanInterval = cleanInterval;
        LifeSpanToken = lifeSpanToken;

        using var db = DbBuilder.BuildPostSqlAndOpen();

        _db = new TokenHandlerDatabase(lastCleanTime, cleanInterval, lifeSpanToken);
        _db.CheckNow(db);

        var response = _db.GetAll(db);

        if (response == EResponse.Err)
            throw new Exception("Try to get Data From Db");
        _ram = new TokenHandlerRam(lastCleanTime, cleanInterval, lifeSpanToken);

        var spanTokenInfo = CollectionsMarshal.AsSpan(response.Ok());
        _ram.SetOverwriteMany(spanTokenInfo);
    }

    public DateTime LastCleanTime { get; private set; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }


    public Response<List<TokenInfoWithGuid>> GetAll(SavePoco db) {
        return _ram.GetAll();
    }

    public void SetOverwrite(SavePoco db, TokenInfoWithGuid tokenInfoWithGuid) {
        _ram.SetOverwrite(tokenInfoWithGuid);
        _db.SetOverwrite(db, tokenInfoWithGuid);
    }

    public void SetOverwriteMany(SavePoco db, Span<TokenInfoWithGuid> span) {
        _ram.SetOverwriteMany(span);
        _db.SetOverwriteMany(db, span);
    }

    public void CheckNow(SavePoco db) {
        _ram.CheckNow();
        _db.CheckNow(db);
    }

    public void RemoveAllTokenWithSameUserId(SavePoco db, long userId) {
        _ram.RemoveAllTokenWithSameUserId(userId);
        _db.RemoveAllTokenWithSameUserId(db, userId);
    }


    public bool TokenExist(SavePoco db, Guid token) {
        var exist = _ram.TokenExist(token);
        if (exist)
            return true;
        _db.RemoveToken(db, token);
        return true;
    }

    public Guid Insert(SavePoco db, long userId) {
        var res = _ram.Insert(userId);
        _db.SetOverwrite(db, new TokenInfoWithGuid {
            TokenInfo = new TokenInfo { CreateDay = DateTime.UtcNow, UserId = userId },
            Token = res
        });
        return res;
    }

    public Response Refresh(SavePoco db, Guid token) {
        _ram.Refresh(token);
        return _db.Refresh(db, token);
    }

    public void RemoveToken(SavePoco db, Guid token) {
        _ram.RemoveToken(token);
        _db.RemoveToken(db, token);
    }

    public Response<TokenInfo> GetTokenInfo(SavePoco db, Guid token) {
        return _ram.GetTokenInfo(token);
    }

    public void RemoveDeadTokenIfNextCleanDate(SavePoco db) {
        if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
        LastCleanTime = DateTime.UtcNow;
        Task.Factory.StartNew(() => { RemoveDeadToken(db); });
    }

    public void RemoveDeadToken(SavePoco db) {
        _ram.RemoveDeadToken();
        _db.RemoveDeadToken(db);
    }

    public void RefreshAuto() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        RemoveDeadTokenIfNextCleanDate(db);
    }
}