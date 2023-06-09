using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2;

public class Api2Login : ControllerExtensions {
    [HttpPost("/api2/token-create")]
    [PrivilegeRoute(route: "/api2/token-create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateApi2TokenResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateApi2TokenAsync([FromBody] CreateApi2TokenProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var userOption = (await log.AddResultAndTransformAsync(await QueryUserInfo
                .GetIdUsernamePasswordByLowerUsernameAsync(db, prop.Username ?? "")))
                .OkOr(Option<UserInfo>.Empty);
            
            
            if (userOption.IsNotSet()) {
                await log.AddLogDebugAsync("User Not Found");
                return Ok(new CreateApi2TokenResult {
                    Token = Guid.Empty,
                    PasswdFalse = false,
                    UsernameFalse = true
                });
            }
            
            var user = userOption.Unwrap();
            if (user.Password != ToPasswdHash(prop.Passwd ?? string.Empty)) {
                await log.AddLogDebugAsync("User Password False");
                return Ok(new CreateApi2TokenResult {
                    Token = Guid.Empty,
                    PasswdFalse = true,
                    UsernameFalse = false
                });
            }

            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var optionToken = await log.AddResultAndTransformAsync(
                await tokenHandler.InsertAsync(db, user.UserId));
            
            if (optionToken == EResult.Err) {
                return this.GetInternalServerError();
            }

            await log.AddLogDebugAsync("Return Token");
            return Ok(new CreateApi2TokenResult {
                Token = optionToken.Ok(),
                PasswdFalse = false,
                UsernameFalse = false
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

    [HttpPost("/api2/token-refresh")]
    [PrivilegeRoute(route: "/api2/token-refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshApi2TokenAsync([FromBody] SimpleTokenProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var optionExistResult = (await log.AddResultAndTransformAsync(await tokenHandler.TokenExistAsync(db, prop.Token)));
            if (optionExistResult == EResult.Err)
                return BadRequest();
            
            var optionExist = optionExistResult.Ok();
            if (optionExist == false)
                return Ok(new ApiTypes.Work { HasWork = false });

            
            var resultErr = await log.AddResultAndTransformAsync<ResultErr<string>>(await tokenHandler.RefreshAsync(db, prop.Token));
            return Ok(resultErr == EResult.Err 
                ? new ApiTypes.Work { HasWork = false } 
                : new ApiTypes.Work { HasWork = true });
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

    [HttpPost("/api2/token-remove")]
    [PrivilegeRoute(route: "/api2/token-remove")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveApi2Token([FromBody] SimpleTokenProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        try {

            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var resultErr = await log.AddResultAndTransformAsync<ResultErr<string>>(await tokenHandler
                .RemoveTokenAsync(db, prop.Token));

            return Ok(resultErr == EResult.Err 
                ? new ApiTypes.Work { HasWork = false } 
                : new ApiTypes.Work { HasWork = true });
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

    [HttpPost("/api2/token-user-id")]
    [PrivilegeRoute(route: "/api2/token-user-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<long>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTokenUserId([FromBody] SimpleTokenProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var optionResp = (await log.AddResultAndTransformAsync(await tokenHandler.GetTokenInfoAsync(db, prop.Token))).OkOr(Option<TokenInfo>.Empty);
            return optionResp.IsSet() == false
                ? Ok(new ApiTypes.ExistOrFoundInfo<long> { Value = -1, ExistOrFound = false })
                : Ok(new ApiTypes.ExistOrFoundInfo<long> { Value = optionResp.Unwrap().UserId, ExistOrFound = true });
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
    public class CreateApi2TokenProp {
        public string? Username { get; set; }
        public string? Passwd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CreateApi2TokenResult {
        public required Guid Token { get; set; }
        public required bool UsernameFalse { get; set; }
        public required bool PasswdFalse { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SimpleTokenProp {
        public Guid Token { get; set; }
    }
}
