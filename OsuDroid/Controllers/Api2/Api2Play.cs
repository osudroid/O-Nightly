using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2;

public class Api2Play : ControllerExtensions {
    [HttpPost("/api2/play/by-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayInfoById))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public IActionResult GetPlayById([FromBody] ApiTypes.Api2GroundWithHash<Api2PlayById> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        if (prop.HashValidate() == false)
            return BadRequest(prop.PrintHashOrder());

        
        var optionRep = log.AddResultAndTransform(ScorePack.GetByPlayId(prop.Body!.PlayId))
            .OkOr(Option<(BblScore Score, string Username, string Region)>.Empty);
        
        return optionRep.IsSet() == false
            ? BadRequest("Not Found")
            : Ok(new PlayInfoById {
                Region = optionRep.Unwrap().Region,
                Score = optionRep.Unwrap().Score,
                Username = optionRep.Unwrap().Username
            });
    }

    [HttpPost("/api2/play/recent")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ExistOrFoundInfo<IReadOnlyList<PlayRecent.BblScoreWithUsername>>))]
    public async Task<IActionResult> GetRecentPlay([FromBody] ApiTypes.Api2GroundNoHeader<RecentPlays> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        await log.AddLogDebugStartAsync();
        
        if (prop.ValuesAreGood() == false)
            return BadRequest();
        try {
            var repTask = PlayRecent.FilterByAsync(
                prop.Body!.FilterPlays!,
                prop.Body!.OrderBy!,
                prop.Body!.Limit,
                prop.Body!.StartAt
            );

            if (await Task.WhenAny(repTask, Task.Delay(3000)) != repTask)
                // timeout logic
                return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<PlayRecent.BblScoreWithUsername>> {
                    Value = ArraySegment<PlayRecent.BblScoreWithUsername>.Empty,
                    ExistOrFound = false
                });
            // task completed within timeout
            return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<PlayRecent.BblScoreWithUsername>> {
                Value = repTask.Result,
                ExistOrFound = true
            });
        }
        catch (Exception) {
            return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<PlayRecent.BblScoreWithUsername>> {
                Value = ArraySegment<PlayRecent.BblScoreWithUsername>.Empty,
                ExistOrFound = false
            });
        }
    }


    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class RecentPlays : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        public string? FilterPlays { get; set; }
        public string? OrderBy { get; set; }
        public int Limit { get; set; }
        public int StartAt { get; set; }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(FilterPlays),
                nameof(OrderBy),
                nameof(Limit),
                nameof(StartAt)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] {
                FilterPlays??"",
                OrderBy??"",
                Limit,
                StartAt
            });
        }

        public bool ValuesAreGood() {
            return ValidateFilterPlays()
                   && ValidateOrderBy()
                   && ValidateLimit()
                   && ValidateStartAt();
        }

        private bool ValidateFilterPlays() {
            return FilterPlays switch {
                "Any"
                    or "XSS_Plays"
                    or "SS_Plays"
                    or "XS_Plays"
                    or "S_Plays"
                    or "A_Plays"
                    or "B_Plays"
                    or "C_Plays"
                    or "D_Plays"
                    or "Accuracy_100"
                    => true,
                _ => false
            };
        }

        private bool ValidateOrderBy() {
            return OrderBy switch {
                "Time_ASC"
                    or "Time_DESC"
                    or "Score_ASC"
                    or "Score_DESC"
                    or "Combo_ASC"
                    or "Combo_DESC"
                    or "50_ASC"
                    or "50_DESC"
                    or "100_ASC"
                    or "100_DESC"
                    or "300_ASC"
                    or "300_DESC"
                    or "Miss_ASC"
                    or "Miss_DESC"
                    => true,
                _ => false
            };
        }

        private bool ValidateLimit() {
            if (Limit > 100 || Limit < 1)
                return false;
            return true;
        }

        private bool ValidateStartAt() {
            if (StartAt < 0)
                return false;
            return true;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PlayInfoById {
        public BblScore? Score { get; set; }
        public string? Username { get; set; }
        public string? Region { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class Api2PlayById : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        public long PlayId { get; set; }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(PlayId)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new[] { PlayId.ToString() });
        }

        public bool ValuesAreGood() {
            return PlayId > -1;
        }
    }
}