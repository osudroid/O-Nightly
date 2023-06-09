using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Query;

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

    public async Task<Result<List<TokenInfoWithGuid>, string>> GetAllAsync(NpgsqlConnection db) {
        return (await QueryTokenUser.GetAllTokensAsync(db))
            .Map(x => x.Select(f => (TokenInfoWithGuid)f).ToList());
    }

    public async Task<ResultErr<string>> SetOverwriteAsync(NpgsqlConnection db, TokenInfoWithGuid tokenInfoWithGuid) {
        return await QueryTokenUser.CreateOrUpdateAsync(
            db: db, 
            createDay: tokenInfoWithGuid.TokenInfo.CreateDay,
            userId: tokenInfoWithGuid.TokenInfo.UserId,
            token: tokenInfoWithGuid.Token
        );
    }

    public async Task<ResultErr<string>> SetOverwriteManyAsync(NpgsqlConnection db, IEnumerable<TokenInfoWithGuid> iter) {
        foreach (var tokenInfoWithGuid in iter) {
            var result = await SetOverwriteAsync(db, tokenInfoWithGuid);
            if (result == EResult.Err)
                return result;    
        }
        
        return ResultErr<string>.Ok();
    }

    public async Task<ResultErr<string>> CheckNowAsync(NpgsqlConnection db) {
        LastCleanTime = DateTime.UtcNow;
        return await RemoveDeadTokenAsync(db);
    }

    public async Task<ResultErr<string>> RemoveAllTokenWithSameUserIdAsync(NpgsqlConnection db, long userId) {
        return await QueryTokenUser.DeleteManyByUserIdAsync(db, userId);
    }


    public async Task<Result<bool, string>> TokenExistAsync(NpgsqlConnection db, Guid token) {
        var result = await QueryTokenUser.GetByTokenAsync(db, token);
        if (result == EResult.Err)
            return result.ChangeOkType<bool>();
        
        if (result.Ok().IsNotSet())
            return Result<bool, string>.Ok(false);

        var tokenUser = result.Ok().Unwrap();
        return (await ReturnOrDeleteIfDead(db, tokenUser))
            .Map(x => x.Delete == false);
    }

    public async Task<Result<Guid, string>> InsertAsync(NpgsqlConnection db, long userId) {
        var guid = Guid.NewGuid();
        return (await db.SafeInsertAsync(new TokenUser {
            UserId = userId,
            CreateDate = DateTime.UtcNow,
            TokenId = guid
        })).Map(x => guid);
    }

    public async Task<ResultErr<string>> RefreshAsync(NpgsqlConnection db, Guid token) {
        return await QueryTokenUser.UpdateCreateTimeAsync(db, token, DateTime.UtcNow);
    }

    public async Task<ResultErr<string>> RemoveTokenAsync(NpgsqlConnection db, Guid token) {
        return await QueryTokenUser.DeleteByTokenIdAsync(db, token);
    }

    public async Task<Result<Option<TokenInfo>, string>> GetTokenInfoAsync(NpgsqlConnection db, Guid token) {
        return await (await QueryTokenUser.GetByTokenAsync(db, token))
            .MapAsync(async x => {
                if (x.IsNotSet())
                    return Option<TokenInfo>.Empty;
                var s = (await ReturnOrDeleteIfDead(db, x.Unwrap())).Map(f => f.tokenInfo);
                if (s == EResult.Err)
                    return Option<TokenInfo>.Empty;
                return Option<TokenInfo>.With(s.Ok());
            });
    }

    public async Task RemoveDeadTokenIfNextCleanDateAsync(NpgsqlConnection db) {
        if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
        LastCleanTime = DateTime.UtcNow;
        await RemoveDeadTokenAsync(db);
    }

    public async Task<ResultErr<string>> RemoveDeadTokenAsync(NpgsqlConnection db) {
        var time = DateTime.UtcNow.Subtract(LifeSpanToken);
        return await QueryTokenUser.DeleteOlderEqThen(db, time);
    }

    public async Task RefreshAutoAsync() {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        await RemoveDeadTokenIfNextCleanDateAsync(db);
    }

    private bool IsDead(TokenUser tokenUser) {
        return tokenUser.CreateDate.Add(LifeSpanToken) < DateTime.UtcNow;
    }

    private async Task<Result<(bool Delete, TokenInfo tokenInfo), string>> ReturnOrDeleteIfDead(NpgsqlConnection db, TokenUser tokenUser) {
        if (IsDead(tokenUser) == false)
            return Result<(bool Delete, TokenInfo tokenInfo), string>.Ok((false, (TokenInfo)tokenUser));
        
        
        var result = await QueryTokenUser.DeleteByTokenIdAsync(db, tokenUser.TokenId);
            
        if (result == EResult.Err)
            return Result<(bool Delete, TokenInfo tokenInfo), string>.Err(result.Err());
        return Result<(bool Delete, TokenInfo tokenInfo), string>.Ok((true, (TokenInfo)tokenUser));

    }
}