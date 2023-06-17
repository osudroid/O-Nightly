using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Class;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Submit : ControllerExtensions {
    [HttpPost("/api2/submit/play-start")]
    [PrivilegeRoute(route: "/api2/submit/play-start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPushPlayStartResult200))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushPlayStart([FromBody] OsuDroid.Post.Api2.PostApi2GroundWithHash<PostPushPlayStart> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest();

            if (prop.HashValidate() == false)
                return BadRequest(prop.PrintHashOrder());


            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var tokenInfoResp = (await log
                    .AddResultAndTransformAsync(await tokenHandler.GetTokenInfoAsync(db, prop.Header!.Token)))
                .OkOr(Option<TokenInfo>.Empty);

            if (tokenInfoResp.IsSet() == false)
                return BadRequest("Token Error");

            var resp = await log.AddResultAndTransformAsync(await Submit.InsertPreBuildPlayAsync(
                db,
                tokenInfoResp.Unwrap().UserId,
                prop.Body!.Filename!,
                prop.Body!.FileHash!
            ));

            return resp == EResult.Err
                ? GetInternalServerError()
                : Ok(new ViewPushPlayStartResult200 { PlayId = resp.Ok() });
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

    [HttpPost("/api2/submit/play-end")]
    [PrivilegeRoute(route: "/api2/submit/play-end")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPushReplayResult200))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushReplay([FromBody] OsuDroid.Post.Api2.PostApi2GroundWithHash<PostPushPlay> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                return BadRequest();
            }
            
            if (prop.HashValidate() == false)
                return BadRequest(prop.PrintHashOrder());

            var tokenHandlerManager = TokenHandlerManger.GetOrCreateCacheDatabase();
            var tokenInfoResp = (await log
                    .AddResultAndTransformAsync(await tokenHandlerManager.GetTokenInfoAsync(db, prop.Header!.Token)))
                .OkOr(Option<TokenInfo>.Empty);

            if (tokenInfoResp.IsSet() == false)
                return BadRequest("Token Error");

            var resp = (await log
                    .AddResultAndTransformAsync(await Submit
                        .InsertFinishPlayAndUpdateUserScoreAsync(db, tokenInfoResp.Unwrap().UserId, prop.Body!)))
                .OkOr(Option<(UserStats userStats, long BestPlayScoreId)>.Empty);

            return resp.IsSet() == false
                ? GetInternalServerError()
                : Ok(new ViewPushReplayResult200 {
                    UserStats = ViewUserStats.FromUserStats(resp.Unwrap().userStats),
                    BestPlayScoreId = resp.Unwrap().BestPlayScoreId
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

    [HttpPost("/api2/submit/replay-file")]
    [PrivilegeRoute(route: "/api2/submit/replay-file")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadReplayFile([FromForm] PostApi2UploadReplayFilePropAsFormWrapper form) {
        PostApi.PostApi2GroundWithHash<PostApi2UploadReplayFile> prop;

        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            try {
                var value =
                    JsonConvert.DeserializeObject<PostApi.PostApi2GroundWithHash<PostApi2UploadReplayFile>>(form.Prop ?? "");
                if (value is null)
                    return BadRequest("JSON is false");
                prop = value;
            }
            catch (Exception e) {
                await log.AddLogErrorAsync(e.ToString());
                return BadRequest("JSON is false");
            }

            if (prop.ValuesAreGood() == false)
                return BadRequest("Values Are Values");

            if (prop.HashValidate() == false)
                return BadRequest(prop.PrintHashOrder());

            if (form.File is null)
                return NotFound("File Not Found In Form");

            var tokenInfoResult = await log
                .AddResultAndTransformAsync(await TokenHandlerManger.GetOrCreateCacheDatabase()
                                                                    .GetTokenInfoAsync(db, prop.Header!.Token));

            if (tokenInfoResult == EResult.Err)
                return GetInternalServerError();
            
            if (tokenInfoResult.Ok().IsNotSet())
                return BadRequest("Token Dead Or Error");

            
            var resp = await log.AddResultAndTransformAsync(await Upload.UploadReplayAsync(
                db,
                prop.Body!.MapHash ?? "",
                prop.Body!.ReplayId,
                tokenInfoResult.Ok().Unwrap().UserId,
                form.File)
            );

            return resp == EResult.Err 
                ? GetInternalServerError() 
                : Ok(resp.Ok());
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
