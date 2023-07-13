using System.Runtime.InteropServices;
using Npgsql;

namespace OsuDroidLib.Manager.TokenHandler;

// public class TokenHandlerRamAndDb : ITokenHandlerDb {
//     private readonly ITokenHandlerDb _db;
//     private readonly ITokenHandler _ram;
//
//     public TokenHandlerRamAndDb(DateTime lastCleanTime, TimeSpan cleanInterval, TimeSpan lifeSpanToken) {
//         LastCleanTime = lastCleanTime;
//         CleanInterval = cleanInterval;
//         LifeSpanToken = lifeSpanToken;
//
//         using var db = DbBuilder.BuildNpgsqlConnection();
//
//         _db = new TokenHandlerDatabase(lastCleanTime, cleanInterval, lifeSpanToken);
//         _db.CheckNowAsync(db);
//         
//         var response = _db.GetAllAsync(db);
//
//         if (response == EResult.Err)
//             throw new Exception("Try to get Data From Db");
//         _ram = new TokenHandlerRam(lastCleanTime, cleanInterval, lifeSpanToken);
//
//         var spanTokenInfo = CollectionsMarshal.AsSpan(response.Ok());
//         _ram.SetOverwriteMany(spanTokenInfo);
//     }
//
//     public DateTime LastCleanTime { get; private set; }
//     public TimeSpan CleanInterval { get; set; }
//     public TimeSpan LifeSpanToken { get; set; }
//
//
//     public async Task<Result<List<TokenInfoWithGuid>, string>> GetAllAsync(NpgsqlConnection db) {
//         return _ram.GetAll();
//     }
//
//     public async Task<ResultErr<string>> SetOverwriteAsync(NpgsqlConnection db, TokenInfoWithGuid tokenInfoWithGuid) {
//         _ram.SetOverwrite(tokenInfoWithGuid);
//         return _db.SetOverwriteAsync(db, tokenInfoWithGuid);
//     }
//
//     public async Task<ResultErr<string>> SetOverwriteManyAsync(NpgsqlConnection db, Span<TokenInfoWithGuid> span) {
//         _ram.SetOverwriteMany(span);
//         return _db.SetOverwriteManyAsync(db, span);
//     }
//
//     public async Task<ResultErr<string>> CheckNowAsync(NpgsqlConnection db) {
//         _ram.CheckNow();
//         return _db.CheckNowAsync(db);
//     }
//
//     public async Task<ResultErr<string>> RemoveAllTokenWithSameUserIdAsync(NpgsqlConnection db, long userId) {
//         _ram.RemoveAllTokenWithSameUserId(userId);
//         return _db.RemoveAllTokenWithSameUserIdAsync(db, userId);
//     }
//
//
//     public async Task<Result<bool, string>> TokenExistAsync(NpgsqlConnection db, Guid token) {
//         var exist = _ram.TokenExist(token);
//         if (exist)
//             return Result<bool, string>.Ok(true);
//         var resultErr = _db.RemoveTokenAsync(db, token);
//         if (resultErr == EResult.Err)
//             return Result<bool, string>.Err(resultErr.Err());
//         return Result<bool, string>.Ok(false);
//     }
//
//     public async Task<Result<Guid, string>> InsertAsync(NpgsqlConnection db, long userId) {
//         var res = _ram.Insert(userId);
//         var resultErr = _db.SetOverwriteAsync(db, new TokenInfoWithGuid {
//             TokenInfo = new TokenInfo { CreateDay = DateTime.UtcNow, UserId = userId },
//             Token = res
//         });
//         if (resultErr == EResult.Err)
//             return Result<Guid, string>.Err(resultErr.Err());
//         return Result<Guid, string>.Ok(res);
//     }
//
//     public async Task<ResultErr<string>> RefreshAsync(NpgsqlConnection db, Guid token) {
//         var x = _ram.Refresh(token);
//         if (x == EResult.Err) return x;
//         return _db.RefreshAsync(db, token);
//     }
//
//     public async Task<ResultErr<string>> RemoveTokenAsync(NpgsqlConnection db, Guid token) {
//         _ram.RemoveToken(token);
//         return _db.RemoveTokenAsync(db, token);
//     }
//
//     public async Task<Result<Option<TokenInfo>, string>> GetTokenInfoAsync(NpgsqlConnection db, Guid token) {
//         return Result<Option<TokenInfo>, string>.Ok(_ram.GetTokenInfo(token));
//     }
//
//     public async Task RemoveDeadTokenIfNextCleanDateAsync(NpgsqlConnection db) {
//         if (LastCleanTime.Add(CleanInterval) > DateTime.UtcNow) return;
//         LastCleanTime = DateTime.UtcNow;
//         Task.Factory.StartNew(() => { RemoveDeadTokenAsync(db); });
//     }
//
//     public async Task<ResultErr<string>> RemoveDeadTokenAsync(NpgsqlConnection db) {
//         _ram.RemoveDeadToken();
//         return _db.RemoveDeadTokenAsync(db);
//     }
//
//     public void RefreshAuto() {
//         using var db = DbBuilder.BuildPostSqlAndOpen();
//         RemoveDeadTokenIfNextCleanDateAsync(db);
//     }
// }