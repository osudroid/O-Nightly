using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    public Response<List<TokenInfoWithGuid>> GetAll(SavePoco db) {
        var response = db.Fetch<BblTokenUser>();
        return response.Status switch {
            EResponse.Err => Response<List<TokenInfoWithGuid>>.Err,
            _ => Response<List<TokenInfoWithGuid>>.Ok(response.Ok().Select(x => (TokenInfoWithGuid)x).ToList())
        };
    }

    public void SetOverwrite(SavePoco db, TokenInfoWithGuid tokenInfoWithGuid) {
        var sql = new Sql(@$"
INSERT 
INTO bbl_token_user (token_id, user_id, create_date) 
VALUES ('{tokenInfoWithGuid.Token}', {tokenInfoWithGuid.TokenInfo.UserId}, '{Time.ToScyllaString(tokenInfoWithGuid.TokenInfo.CreateDay)}') 
ON CONFLICT (token_id) DO UPDATE 
set create_date = '{Time.ToScyllaString(tokenInfoWithGuid.TokenInfo.CreateDay)}',
    user_id = {tokenInfoWithGuid.TokenInfo.UserId}
", tokenInfoWithGuid.Token, tokenInfoWithGuid.TokenInfo.UserId);
        db.Execute(sql);
    }

    public void SetOverwriteMany(SavePoco db, Span<TokenInfoWithGuid> span) {
        ref var searchSpace = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < span.Length; i++) {
            var iteam = Unsafe.Add(ref searchSpace, i);
            SetOverwrite(db, iteam);
        }
    }

    public void CheckNow(SavePoco db) {
        LastCleanTime = DateTime.UtcNow;
        RemoveDeadToken(db);
    }

    public void RemoveAllTokenWithSameUserId(SavePoco db, long userId) {
        db.Execute(@$"
DELETE FROM bbl_token_user
WHERE user_id = {userId}
");
    }


    public bool TokenExist(SavePoco db, Guid token) {
        var response = db.First<BblTokenUser>(@"
SELECT *
FROM bbl_token_user
WHERE token_id = @0
", token);

        if (response.Status == EResponse.Err)
            return false;

        return ReturnOrDeleteIfDead(db, response.Ok()) switch {
            { Delete: true } => false,
            _ => true
        };
    }

    public Guid Insert(SavePoco db, long userId) {
        var guid = Guid.NewGuid();
        db.Insert(new BblTokenUser {
            UserId = userId,
            CreateDate = DateTime.UtcNow,
            TokenId = guid
        });
        return guid;
    }

    public Response Refresh(SavePoco db, Guid token) {
        return (Response)db.Execute(@$"
UPDATE bbl_token_user
SET create_date = '{Time.ToScyllaString(DateTime.UtcNow)}'
WHERE token_id = @0 
", token);
    }

    public void RemoveToken(SavePoco db, Guid token) {
        db.Execute("DELETE FROM bbl_token_user WHERE token_id = @0", token);
    }

    public Response<TokenInfo> GetTokenInfo(SavePoco db, Guid token) {
        var responseFetch = db.Fetch<BblTokenUser>("SELECT * FROM bbl_token_user WHERE token_id = @0", token);
        if (responseFetch == EResponse.Err)
            return Response<TokenInfo>.Err;
        var list = responseFetch.Ok() ?? new List<BblTokenUser>();
        if (list.Count == 0) return Response<TokenInfo>.Err;
        return ReturnOrDeleteIfDead(db, list[0]) switch {
            { Delete: true } => Response<TokenInfo>.Err,
            { Delete: false } v => Response<TokenInfo>.Ok(v.tokenInfo)
        };
    }

    public void RemoveDeadTokenIfNextCleanDate(SavePoco db) {
        if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
        LastCleanTime = DateTime.UtcNow;
        Task.Factory.StartNew(() => { RemoveDeadToken(db); });
    }

    public void RemoveDeadToken(SavePoco db) {
        var time = DateTime.UtcNow.Subtract(LifeSpanToken);
        db.Execute(@$"
DELETE FROM bbl_token_user
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

    private (bool Delete, TokenInfo tokenInfo) ReturnOrDeleteIfDead(SavePoco db, BblTokenUser bblTokenUser) {
        if (IsDead(bblTokenUser)) {
            db.Execute(@"
DELETE FROM bbl_token_user
WHERE token_id = @0
", bblTokenUser.TokenId);
            return (true, (TokenInfo)bblTokenUser);
        }

        return (false, (TokenInfo)bblTokenUser);
    }
}