using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Submit : ControllerExtensions {
    [HttpPost("/api2/submit/play-start")]
    [PrivilegeRoute(route: "/api2/submit/play-start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PushPlayStartResult200))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushPlayStart([FromBody] ApiTypes.Api2GroundWithHash<PushPlayStartProp> prop) {
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
                : Ok(new PushPlayStartResult200 { PlayId = resp.Ok() });
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PushReplayResult200))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushReplay([FromBody] ApiTypes.Api2GroundWithHash<PushPlayProp> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest();

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
                : Ok(new PushReplayResult200 {
                    UserStats = resp.Unwrap().userStats,
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadReplayFile([FromForm] Api2UploadReplayFilePropAsFormWrapper form) {
        ApiTypes.Api2GroundWithHash<Api2UploadReplayFileProp> prop;

        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            try {
                var value =
                    JsonConvert.DeserializeObject<ApiTypes.Api2GroundWithHash<Api2UploadReplayFileProp>>(form.Prop ?? "");
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

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class Api2UploadReplayFilePropAsFormWrapper {
        public IFormFile? File { get; set; }
        public string? Prop { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PushReplayResult200 {
        public UserStats? UserStats { get; set; }
        public long BestPlayScoreId { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PushPlayStartResult200 {
        public long PlayId { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PushPlayStartProp : Submit.ScoreProp, ApiTypes.IValuesAreGood, ApiTypes.ISingleString,
        ApiTypes.IPrintHashOrder {
        public string? Filename { get; set; }
        public string? FileHash { get; set; }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(Filename),
                nameof(FileHash)
            });
        }

        public string ToSingleString() {
            return Merge
                .ObjectsToString(new Object[] {
                    Filename ?? "",
                    FileHash ?? ""
                });
        }

        public bool ValuesAreGood() {
            return string.IsNullOrEmpty(Filename) != true
                   && string.IsNullOrEmpty(FileHash) != true
                ;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PushPlayProp : Submit.ScoreProp, ApiTypes.IValuesAreGood, ApiTypes.ISingleString,
        ApiTypes.IPrintHashOrder {
        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(Mode),
                nameof(Mark),
                nameof(Id),
                nameof(Score),
                nameof(Combo),
                nameof(Uid),
                nameof(Geki),
                nameof(Perfect),
                nameof(Katu),
                nameof(Good),
                nameof(Bad),
                nameof(Miss),
                nameof(Accuracy)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] {
                Mode ?? "",
                Mark ?? "",
                Id,
                Score,
                Combo,
                Uid,
                Geki,
                Perfect,
                Katu,
                Good,
                Bad,
                Miss,
                Accuracy
            });
        }

        public bool ValuesAreGood() {
            return
                string.IsNullOrEmpty(Mode) != true &&
                string.IsNullOrEmpty(Mark) != true &&
                Id != -1 &&
                Score != -1 &&
                Combo != -1 &&
                Uid != -1 &&
                Geki != -1 &&
                Perfect != -1 &&
                Katu != -1 &&
                Good != -1 &&
                Bad != -1 &&
                Miss != -1 &&
                Accuracy != -1
                ;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class Api2UploadReplayFileProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        public string? MapHash { get; set; }
        public long ReplayId { get; set; }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(MapHash),
                nameof(ReplayId)
            });
        }

        public string ToSingleString() {
            return Merge.ListToString(new object[] {
                MapHash ?? "", ReplayId
            });
        }

        public bool ValuesAreGood() {
            return
                string.IsNullOrEmpty(MapHash) == false
                && ReplayId > -1;
        }
    }
}
