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

        if (response == EResult.Err)
            throw new Exception("Try to get Data From Db");
        _ram = new TokenHandlerRam(lastCleanTime, cleanInterval, lifeSpanToken);

        var spanTokenInfo = CollectionsMarshal.AsSpan(response.Ok());
        _ram.SetOverwriteMany(spanTokenInfo);
    }

    public DateTime LastCleanTime { get; private set; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }


    public Result<List<TokenInfoWithGuid>, string> GetAll(SavePoco db) {
        return _ram.GetAll();
    }

    public ResultErr<string> SetOverwrite(SavePoco db, TokenInfoWithGuid tokenInfoWithGuid) {
        _ram.SetOverwrite(tokenInfoWithGuid);
        return _db.SetOverwrite(db, tokenInfoWithGuid);
    }

    public ResultErr<string> SetOverwriteMany(SavePoco db, Span<TokenInfoWithGuid> span) {
        _ram.SetOverwriteMany(span);
        return _db.SetOverwriteMany(db, span);
    }

    public ResultErr<string> CheckNow(SavePoco db) {
        _ram.CheckNow();
        return _db.CheckNow(db);
    }

    public ResultErr<string> RemoveAllTokenWithSameUserId(SavePoco db, long userId) {
        _ram.RemoveAllTokenWithSameUserId(userId);
        return _db.RemoveAllTokenWithSameUserId(db, userId);
    }


    public Result<bool, string> TokenExist(SavePoco db, Guid token) {
        var exist = _ram.TokenExist(token);
        if (exist)
            return Result<bool, string>.Ok(true);
        var resultErr = _db.RemoveToken(db, token);
        if (resultErr == EResult.Err)
            return Result<bool, string>.Err(resultErr.Err());
        return Result<bool, string>.Ok(false);
    }

    public Result<Guid, string> Insert(SavePoco db, long userId) {
        var res = _ram.Insert(userId);
        var resultErr = _db.SetOverwrite(db, new TokenInfoWithGuid {
            TokenInfo = new TokenInfo { CreateDay = DateTime.UtcNow, UserId = userId },
            Token = res
        });
        if (resultErr == EResult.Err)
            return Result<Guid, string>.Err(resultErr.Err());
        return Result<Guid, string>.Ok(res);
    }

    public ResultErr<string> Refresh(SavePoco db, Guid token) {
        var x = _ram.Refresh(token);
        if (x == EResult.Err) return x;
        return _db.Refresh(db, token);
    }

    public ResultErr<string> RemoveToken(SavePoco db, Guid token) {
        _ram.RemoveToken(token);
        return _db.RemoveToken(db, token);
    }

    public Result<Option<TokenInfo>, string> GetTokenInfo(SavePoco db, Guid token) {
        return Result<Option<TokenInfo>, string>.Ok(_ram.GetTokenInfo(token));
    }

    public void RemoveDeadTokenIfNextCleanDate(SavePoco db) {
        if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
        LastCleanTime = DateTime.UtcNow;
        Task.Factory.StartNew(() => { RemoveDeadToken(db); });
    }

    public ResultErr<string> RemoveDeadToken(SavePoco db) {
        _ram.RemoveDeadToken();
        return _db.RemoveDeadToken(db);
    }

    public void RefreshAuto() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        RemoveDeadTokenIfNextCleanDate(db);
    }
}