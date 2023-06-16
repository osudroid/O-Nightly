using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2;

public class Api2Play : ControllerExtensions {
    [HttpPost("/api2/play/by-id")]
    [PrivilegeRoute(route: "/api2/play/by-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlayInfoById))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> GetPlayById([FromBody] ApiTypes.Api2GroundWithHash<Api2PlayById> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false) {
                await log.AddLogDebugAsync("Values Are Bad");
                return BadRequest();
            }


            if (prop.HashValidate() == false) {
                await log.AddLogDebugAsync("Hash Not Valid");
                return BadRequest(prop.PrintHashOrder());
            }


            await log.AddLogDebugAsync("PlayId: " + prop.Body!.PlayId);
            var optionRep = (await log.AddResultAndTransformAsync(await ScorePack
                    .GetByPlayIdAsync(db, prop.Body!.PlayId)))
                .OkOr(Option<(PlayScore Score, string Username, string Region)>.Empty);

            await (optionRep.IsSet() 
                ? log.AddLogDebugAsync("PlayId Found")
                : log.AddLogDebugAsync("PlayId Not Found"));

            return optionRep.IsSet() == false
                ? BadRequest("Not Found")
                : Ok(new ViewPlayInfoById {
                    Region = optionRep.Unwrap().Region,
                    Score = ViewPlayScore.FromPlayScore(optionRep.Unwrap().Score),
                    Username = optionRep.Unwrap().Username
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

    [HttpPost("/api2/play/recent")]
    [PrivilegeRoute(route: "/api2/play/recent")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>>))]
    public async Task<IActionResult> GetRecentPlay([FromBody] ApiTypes.Api2GroundNoHeader<RecentPlays> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest();
            
            var repTaskResult = await log.AddResultAndTransformAsync(await PlayRecent.FilterByAsync(
                db,
                prop.Body!.FilterPlays!,
                prop.Body!.OrderBy!,
                prop.Body!.Limit,
                prop.Body!.StartAt
            ));

            if (repTaskResult == EResult.Err)
                return GetInternalServerError();

            
            return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>> {
                Value = repTaskResult
                        .Ok()
                        .Select(ViewPlayScoreWithUsername.FromPlayScoreWithUsername).ToList(),
                ExistOrFound = true
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
    public sealed class Api2PlayById : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        public long PlayId { get; set; }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(PlayId)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] { PlayId });
        }

        public bool ValuesAreGood() {
            return PlayId > -1;
        }
    }
}
