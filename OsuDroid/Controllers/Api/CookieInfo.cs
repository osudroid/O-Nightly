using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api;

public sealed class CookieInfo : ControllerExtensions {
    [HttpGet("/api/user-info-by-cookie")]
    [PrivilegeRoute(route: "/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<UserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ExistOrFoundInfo<UserInfo>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfoByCookie() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var tokenInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var userInfoResult = await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetByUserIdAsync(db, tokenInfo.UserId));

            if (userInfoResult == EResult.Err)
                return GetInternalServerError();

            if (userInfoResult.Ok().IsNotSet())
                return NotFound();

            var userInfo = userInfoResult.Ok().Unwrap();
            
            // TODO Check is Supporter
            return Ok(new ApiTypes.ExistOrFoundInfo<UserInfo> {
                ExistOrFound = true,
                Value = new UserInfo {
                    Active = userInfo.Active,
                    Banned = userInfo.Banned,
                    Email = userInfo.Email,
                    Id = userInfo.UserId,
                    Region = userInfo.Region,
                    Username = userInfo.Username,
                    RegistTime = userInfo.RegisterTime,
                    RestrictMode = userInfo.RestrictMode,
                     
                }
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UserInfo {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public DateTime RegistTime { get; set; }
        public string? Region { get; set; }
        public bool Active { get; set; }
        public bool Supporter { get; set; }
        public bool Banned { get; set; }
        public bool RestrictMode { get; set; }
    }
}
public class A: IAsyncDisposable {
    public async ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}