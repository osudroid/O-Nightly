using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Submit : ControllerExtensions {
    [HttpPost("/api2/submit/play-start")]
    [PrivilegeRoute(route: "/api2/submit/play-start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPushPlayStartResult200))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushPlayStart(
        [FromBody] OsuDroid.Post.Api2.PostApi2GroundWithHash<PostPushPlayStart> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (prop.ValuesAreGood() == false)
                return await RollbackAndGetBadRequestAsync(dbT, "Post Prop Are Bad");

            if (prop.HashValidate() == false)
                return await RollbackAndGetBadRequestAsync(dbT, prop.PrintHashOrder());


            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var tokenInfoResp = (await log
                    .AddResultAndTransformAsync(await tokenHandler.GetTokenInfoAsync(db, prop.Header!.Token)))
                .OkOr(Option<TokenInfo>.Empty);

            if (tokenInfoResp.IsSet() == false)
                return await RollbackAndGetBadRequestAsync(dbT, "Token Error");

            var result = await log.AddResultAndTransformAsync(await ModelApi2Submit
                .InsertPreBuildPlayAsync(
                    db,
                    tokenInfoResp.Unwrap().UserId,
                    prop.Body!.Filename!,
                    prop.Body!.FileHash!
                ));

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
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpPost("/api2/submit/play-end")]
    [PrivilegeRoute(route: "/api2/submit/play-end")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPushReplayResult200))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult>
        PushReplay([FromBody] OsuDroid.Post.Api2.PostApi2GroundWithHash<PostPushPlay> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (prop.ValuesAreGood() == false) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                return await RollbackAndGetBadRequestAsync(dbT, "Post Prop Are Bad");
            }

            if (prop.HashValidate() == false)
                return await RollbackAndGetBadRequestAsync(dbT, prop.PrintHashOrder());

            var tokenHandlerManager = TokenHandlerManger.GetOrCreateCacheDatabase();
            var tokenInfoResp = (await log
                    .AddResultAndTransformAsync(await tokenHandlerManager.GetTokenInfoAsync(db, prop.Header!.Token)))
                .OkOr(Option<TokenInfo>.Empty);

            if (tokenInfoResp.IsSet() == false)
                return await RollbackAndGetBadRequestAsync(dbT, "Token Error");

            var result = await log.AddResultAndTransformAsync(await ModelApi2Submit
                .InsertFinishPlayAndUpdateUserScoreAsync(db, tokenInfoResp.Unwrap().UserId, prop.Body!));

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
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
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
        var isComplete = false;
        
        try {
            try {
                var value =
                    JsonConvert.DeserializeObject<PostApi.PostApi2GroundWithHash<PostApi2UploadReplayFile>>(form.Prop ??
                        "");
                if (value is null)
                    return await RollbackAndGetBadRequestAsync(dbT, "JSON is false");
                prop = value;
            }
            catch (Exception e) {
                await log.AddLogErrorAsync(e.ToString());
                return await RollbackAndGetBadRequestAsync(dbT, "JSON is false");
            }

            if (prop.ValuesAreGood() == false)
                return await RollbackAndGetBadRequestAsync(dbT, "Values Are Bad");

            if (prop.HashValidate() == false)
                return await RollbackAndGetBadRequestAsync(dbT, prop.PrintHashOrder());

            if (form.File is null)
                return await RollbackAndGetNotFound(dbT, "File Not Found In Form");

            var tokenInfoResult = await log
                .AddResultAndTransformAsync(await TokenHandlerManger.GetOrCreateCacheDatabase()
                                                                    .GetTokenInfoAsync(db, prop.Header!.Token));

            if (tokenInfoResult == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

            if (tokenInfoResult.Ok().IsNotSet())
                return await RollbackAndGetBadRequestAsync(dbT, "Token Dead Or Error");


            var resp = await log.AddResultAndTransformAsync(await Upload.UploadReplayAsync(
                db,
                prop.Body!.MapHash ?? "",
                prop.Body!.ReplayId,
                tokenInfoResult.Ok().Unwrap().UserId,
                form.File)
            );

            return resp == EResult.Err
                ? await RollbackAndGetInternalServerErrorAsync(dbT)
                : Ok(resp.Ok());
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }
}