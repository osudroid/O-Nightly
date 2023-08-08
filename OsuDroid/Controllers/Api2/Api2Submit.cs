using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.View;
using OsuDroidLib.Class;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Controllers.Api2;

public class Api2Submit : ControllerExtensions {
    [HttpPost("/api2/submit/play-start")]
    [PrivilegeRoute(route: "/api2/submit/play-start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPushPlayStartResult200))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushPlayStart(
        [FromBody] OsuDroid.Post.Api2.PostApi2GroundWithHash<PostPushPlayStart> prop) {
        
                await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = OsuDroidLib.Log.GetLog(db);
        var isComplete = false;
        
        try {
            if (prop.ValuesAreGood() == false) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("Post Prop Are Bad");
            }

            if (prop.HashValidate() == false) {
                await log.AddLogDebugAsync("Hash Is InValid");
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest(prop.PrintHashOrder());
            }


            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var tokenInfoResp = (await log
                    .AddResultAndTransformAsync(await tokenHandler.GetTokenInfoAsync(db, prop.Header!.Token)))
                .OkOr(Option<TokenInfo>.Empty);

            if (tokenInfoResp.IsSet() == false) {
                await log.AddLogDebugAsync("Token Error");
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("Token Error");
            }

            var result = await log.AddResultAndTransformAsync(await ModelApi2Submit
                .InsertPreBuildPlayAsync(
                    db,
                    tokenInfoResp.Unwrap().UserId,
                    prop.Body!.Filename!,
                    prop.Body!.FileHash!
                ));

            if (result == EResult.Err) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return InternalServerError();
            }

            switch (result.Ok().Mode) {
                case EModelResult.Ok:
                    return Ok(result.Ok().Result.Unwrap());
                case EModelResult.BadRequest:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }

                    return BadRequest();
                case EModelResult.InternalServerError:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }
                    return InternalServerError();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e) {
            await log.AddLogErrorAsync(e.ToString());
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }
            return InternalServerError();
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
        
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = OsuDroidLib.Log.GetLog(db);
        var isComplete = false;
        
        try {
            if (prop.ValuesAreGood() == false) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("Post Prop Are Bad");
            }

            if (prop.HashValidate() == false) {
                await log.AddLogDebugAsync("Hash Is InValid");
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest(prop.PrintHashOrder());
            }

            var tokenHandlerManager = TokenHandlerManger.GetOrCreateCacheDatabase();
            var tokenInfoResp = (await log
                    .AddResultAndTransformAsync(await tokenHandlerManager.GetTokenInfoAsync(db, prop.Header!.Token)))
                .OkOr(Option<TokenInfo>.Empty);

            if (tokenInfoResp.IsSet() == false) {
                await log.AddLogDebugAsync("Token Error");
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("Token Error");
            }

            var result = await log.AddResultAndTransformAsync(await ModelApi2Submit
                .InsertFinishPlayAndUpdateUserScoreAsync(db, tokenInfoResp.Unwrap().UserId, prop.Body!));

            if (result == EResult.Err) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return InternalServerError();
            }

            switch (result.Ok().Mode) {
                case EModelResult.Ok:
                    return Ok(result.Ok().Result.Unwrap());
                case EModelResult.BadRequest:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }

                    return BadRequest();
                case EModelResult.InternalServerError:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }
                    return InternalServerError();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e) {
            await log.AddLogErrorAsync(e.ToString());
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }
            return InternalServerError();
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
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = OsuDroidLib.Log.GetLog(db);
        var isComplete = false;
        
        try {
            try {
                var value =
                    JsonConvert.DeserializeObject<PostApi.PostApi2GroundWithHash<PostApi2UploadReplayFile>>(form.Prop ??
                        "");
                if (value is null) {
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }
                    return BadRequest("JSON is false");
                }
                prop = value;
            }
            catch (Exception e) {
                await log.AddLogErrorAsync(e.ToString());
                
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("JSON is false");
            }

            if (prop.ValuesAreGood() == false) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("Values Are Bad");
            }

            if (prop.HashValidate() == false) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest(prop.PrintHashOrder());
            }

            if (form.File is null) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("File Not Found In Form");
            }

            var tokenInfoResult = await log
                .AddResultAndTransformAsync(await TokenHandlerManger.GetOrCreateCacheDatabase()
                                                                    .GetTokenInfoAsync(db, prop.Header!.Token));

            if (tokenInfoResult == EResult.Err) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return InternalServerError();
            }

            if (tokenInfoResult.Ok().IsNotSet()) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return BadRequest("Token Dead Or Error");
            }


            var resp = await log.AddResultAndTransformAsync(await Upload.UploadReplayAsync(
                db,
                prop.Body!.MapHash ?? "",
                prop.Body!.ReplayId,
                tokenInfoResult.Ok().Unwrap().UserId,
                form.File)
            );

            if (resp == EResult.Err) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                return InternalServerError();
            }
            
            return Ok(resp.Ok());
        }
        catch (Exception e) {
            await log.AddLogErrorAsync(e.ToString());
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }
            return InternalServerError();
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }
}