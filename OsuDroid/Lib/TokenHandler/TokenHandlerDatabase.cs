using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http.HttpResults;
using NPoco;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib.TokenHandler;

public class TokenHandlerDatabase : ITokenHandlerDb {
    public TokenHandlerDatabase(DateTime lastCleanTime, TimeSpan cleanInterval, TimeSpan lifeSpanToken) {
        LastCleanTime = lastCleanTime;
        CleanInterval = cleanInterval;
        LifeSpanToken = lifeSpanToken;
    }

    public DateTime LastCleanTime { get; private set; }
    public TimeSpan CleanInterval { get; set; }
    public TimeSpan LifeSpanToken { get; set; }

    public Result<List<TokenInfoWithGuid>, string> GetAll(SavePoco db) {
        return db.Fetch<BblTokenUser>("SELECT * FROM bbl_token_user")
            .Map(x => x.Select(f => (TokenInfoWithGuid)f).ToList());
    }

    public ResultErr<string> SetOverwrite(SavePoco db, TokenInfoWithGuid tokenInfoWithGuid) {
        var sql = new Sql(@$"
INSERT 
INTO bbl_token_user (token_id, user_id, create_date) 
VALUES ('{tokenInfoWithGuid.Token}', {tokenInfoWithGuid.TokenInfo.UserId}, '{Time.ToScyllaString(tokenInfoWithGuid.TokenInfo.CreateDay)}') 
ON CONFLICT (token_id) DO UPDATE 
set create_date = '{Time.ToScyllaString(tokenInfoWithGuid.TokenInfo.CreateDay)}',
    user_id = {tokenInfoWithGuid.TokenInfo.UserId}
", tokenInfoWithGuid.Token, tokenInfoWithGuid.TokenInfo.UserId);
        return db.Execute(sql);
    }

    public ResultErr<string> SetOverwriteMany(SavePoco db, Span<TokenInfoWithGuid> span) {
        ref var searchSpace = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < span.Length; i++) {
            var iteam = Unsafe.Add(ref searchSpace, i);
            var result = SetOverwrite(db, iteam);
            if (result == EResult.Err)
                return result;
        }
        return ResultErr<string>.Ok();
    }

    public ResultErr<string> CheckNow(SavePoco db) {
        LastCleanTime = DateTime.UtcNow;
        return RemoveDeadToken(db);
    }

    public ResultErr<string> RemoveAllTokenWithSameUserId(SavePoco db, long userId) {
        return db.Execute(@$"
DELETE FROM bbl_token_user
WHERE user_id = {userId}
");
    }


    public Result<bool, string> TokenExist(SavePoco db, Guid token) {
        var sql = new Sql(@"
SELECT *
FROM bbl_token_user
WHERE token_id = @0
", token);

        return db.FirstOrDefault<BblTokenUser>(sql)
            .AndThen(user => {
                if (user is null)
                    return Result<bool, string>.Ok(false);
                return ReturnOrDeleteIfDead(db, user).Map(_ => true);
            });
    }

    public Result<Guid, string> Insert(SavePoco db, long userId) {
        var guid = Guid.NewGuid();
        var result = db.Insert(new BblTokenUser {
            UserId = userId,
            CreateDate = DateTime.UtcNow,
            TokenId = guid
        });
        if (result == EResult.Err)
            return Result<Guid, string>.Err(result.Err());
        return Result<Guid, string>.Ok(guid);
    }

    public ResultErr<string> Refresh(SavePoco db, Guid token) {
        return db.Execute(@$"
UPDATE bbl_token_user
SET create_date = '{Time.ToScyllaString(DateTime.UtcNow)}'
WHERE token_id = @0 
", token);
    }

    public ResultErr<string> RemoveToken(SavePoco db, Guid token) {
        return db.Execute("DELETE FROM bbl_token_user WHERE token_id = @0", token);
    }

    public Result<Option<TokenInfo>, string> GetTokenInfo(SavePoco db, Guid token) {
        var sql = new Sql("SELECT * FROM bbl_token_user WHERE token_id = @0", token);
        return db.Fetch<BblTokenUser>(sql).Map(x => {
            if (x.Count == 0)
                return Option<TokenInfo>.Empty;
            var s = ReturnOrDeleteIfDead(db, x[0]).Map(x => x.tokenInfo);
            if (s == EResult.Err)
                return Option<TokenInfo>.Empty;
            return Option<TokenInfo>.With(s.Ok());
        });
    }

    public void RemoveDeadTokenIfNextCleanDate(SavePoco db) {
        if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
        LastCleanTime = DateTime.UtcNow;
        Task.Factory.StartNew(() => { RemoveDeadToken(db); });
    }

    public ResultErr<string> RemoveDeadToken(SavePoco db) {
        var time = DateTime.UtcNow.Subtract(LifeSpanToken);
        return db.Execute(@$"
DELETE 
FROM bbl_token_user
WHERE create_date <= '{Time.ToScyllaString(time)}'
");
    }

    public void RefreshAuto() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        RemoveDeadTokenIfNextCleanDate(db);
    }

    private bool IsDead(BblTokenUser bblTokenUser) {
        return bblTokenUser.CreateDate.Add(LifeSpanToken) < DateTime.UtcNow;
    }

    private Result<(bool Delete, TokenInfo tokenInfo), string> ReturnOrDeleteIfDead(SavePoco db, BblTokenUser bblTokenUser) {
        if (IsDead(bblTokenUser)) {
            return db.Execute(@"
DELETE FROM bbl_token_user
WHERE token_id = @0
", bblTokenUser.TokenId).Map(x => (true, (TokenInfo)bblTokenUser));
        }

        return Result<(bool Delete, TokenInfo tokenInfo), string>.Ok((false, (TokenInfo)bblTokenUser));
    }
}