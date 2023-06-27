using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Post;
using OsuDroid.Class;
using OsuDroid.Model;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2;

public class Api2Login : ControllerExtensions {
    [HttpPost("/api2/token-create")]
    [PrivilegeRoute(route: "/api2/token-create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCreateApi2TokenResult))]
    public async Task<IActionResult> CreateApi2TokenAsync(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostCreateApi2Token> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (!prop.ValuesAreGood()) {
                await log.AddLogDebugAsync("Post Prop Are BadRequest");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var result = await log.AddResultAndTransformAsync(await ModelApi2Login
                .CreateApi2TokenAsync(this, db, log, DtoMapper.CreateApi2TokenToDto(prop.Body!)));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api2/token-refresh")]
    [PrivilegeRoute(route: "/api2/token-refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshApi2TokenAsync(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (!prop.ValuesAreGood()) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var result = await log.AddResultAndTransformAsync(await ModelApi2Login
                .RefreshApi2TokenAsync(this, db, log, DtoMapper.SimpleTokenToDto(prop.Body!)));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api2/token-remove")]
    [PrivilegeRoute(route: "/api2/token-remove")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveApi2Token([FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        try {
            if (!prop.ValuesAreGood()) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var body = prop.Body!;

            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var resultErr = await log.AddResultAndTransformAsync<ResultErr<string>>(await tokenHandler
                .RemoveTokenAsync(db, body.Token));

            return Ok(resultErr == EResult.Err
                ? new ApiTypes.ViewWork { HasWork = false }
                : new ApiTypes.ViewWork { HasWork = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api2/token-user-id")]
    [PrivilegeRoute(route: "/api2/token-user-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<long>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTokenUserId([FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (!prop.ValuesAreGood()) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var body = prop.Body!;

            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var optionResp =
                (await log.AddResultAndTransformAsync(await tokenHandler.GetTokenInfoAsync(db, body.Token))).OkOr(
                    Option<TokenInfo>.Empty);
            return optionResp.IsSet() == false
                ? Ok(new ApiTypes.ViewExistOrFoundInfo<long> { Value = -1, ExistOrFound = false })
                : Ok(new ApiTypes.ViewExistOrFoundInfo<long>
                    { Value = optionResp.Unwrap().UserId, ExistOrFound = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }
}