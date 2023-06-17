using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Class;
using OsuDroidLib.Query;
namespace OsuDroid.Controllers.Api;

public sealed class CookieInfo : ControllerExtensions {
    [HttpGet("/api/user-info-by-cookie")]
    [PrivilegeRoute(route: "/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>))]
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
            return Ok(new ApiTypes.ViewExistOrFoundInfo<ViewUserInfo> {
                ExistOrFound = true,
                Value = new ViewUserInfo {
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
}