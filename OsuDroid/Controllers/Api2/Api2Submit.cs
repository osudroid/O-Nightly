using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OsuDroid.Extensions;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Submit : ControllerExtensions {
    [HttpPost("/api2/submit/play-start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PushPlayStartResult200))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult PushPlayStart([FromBody] ApiTypes.Api2GroundWithHash<PushPlayStartProp> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        if (prop.HashValidate() == false)
            return BadRequest(prop.PrintHashOrder());

        
        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User);
        var tokenInfoResp = log
            .AddResultAndTransform(tokenHandler.GetTokenInfo(db, prop.Header!.Token))
            .OkOr(Option<TokenInfo>.Empty);
        
        if (tokenInfoResp.IsSet() == false)
            return BadRequest("Token Error");

        var resp = log.AddResultAndTransform(Submit.InsertPreBuildPlay(
            tokenInfoResp.Unwrap().UserId,
            prop.Body!.Filename!,
            prop.Body!.FileHash!
        ));

        return resp == EResult.Err 
            ? GetInternalServerError()
            : Ok(new PushPlayStartResult200 { PlayId = resp.Ok() });
    }

    [HttpPost("/api2/submit/play-end")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PushReplayResult200))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult PushReplay([FromBody] ApiTypes.Api2GroundWithHash<PushPlayProp> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        if (prop.HashValidate() == false)
            return BadRequest(prop.PrintHashOrder());
        
        var tokenInfoResp = log
            .AddResultAndTransform(TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User)
            .GetTokenInfo(db, prop.Header!.Token))
            .OkOr(Option<TokenInfo>.Empty);
        
        if (tokenInfoResp.IsSet() == false)
            return BadRequest("Token Error");

        var resp = log
            .AddResultAndTransform(Submit.InsertFinishPlayAndUpdateUserScore(tokenInfoResp.Unwrap().UserId, prop.Body!))
            .OkOr(Option<(BblUserStats userStats, long BestPlayScoreId)>.Empty);
        
        return resp.IsSet() == false
            ? GetInternalServerError()
            : Ok(new PushReplayResult200 {
                UserStats = resp.Unwrap().userStats,
                BestPlayScoreId = resp.Unwrap().BestPlayScoreId
            });
    }

    [HttpPost("/api2/submit/replay-file")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UploadReplayFile([FromForm] Api2UploadReplayFilePropAsFormWrapper form) {
        ApiTypes.Api2GroundWithHash<Api2UploadReplayFileProp> prop;
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        
        try {
            var value =
                JsonConvert.DeserializeObject<ApiTypes.Api2GroundWithHash<Api2UploadReplayFileProp>>(form.Prop ?? "");
            if (value is null)
                return BadRequest("JSON is false");
            prop = value;
        }
        catch (Exception e) {
            log.AddLogError(e.ToString());
            return BadRequest("JSON is false");
        }

        if (prop.ValuesAreGood() == false)
            return BadRequest();

        if (prop.HashValidate() == false)
            return BadRequest(prop.PrintHashOrder());

        
        var tokenInfoResp = log
            .AddResultAndTransform(TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User)
            .GetTokenInfo(db, prop.Header!.Token))
            .OkOr(Option<TokenInfo>.Empty);
        
        if (tokenInfoResp.IsSet() == false)
            return BadRequest("Token Error");

        var resp = Upload.UploadReplay(
            prop.Body!.MapHash ?? "",
            prop.Body!.ReplayId,
            tokenInfoResp.Unwrap().UserId,
            form.File!);

        if (resp == EResult.Err)
            log.AddLogError(resp.Err());
        
        return resp == EResult.Err
            ? BadRequest(resp.Err())
            : Ok(resp.Ok());
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class Api2UploadReplayFilePropAsFormWrapper {
        public IFormFile? File { get; set; }
        public string? Prop { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PushReplayResult200 {
        public BblUserStats? UserStats { get; set; }
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