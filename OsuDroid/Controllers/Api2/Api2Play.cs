using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Class;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2;

public class Api2Play : ControllerExtensions {
    [HttpPost("/api2/play/by-id")]
    [PrivilegeRoute(route: "/api2/play/by-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlayInfoById))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> GetPlayById([FromBody] PostApi.PostApi2GroundWithHash<PostApi2PlayById> prop) {
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
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>>))]
    public async Task<IActionResult> GetRecentPlay([FromBody] PostApi.PostApi2GroundNoHeader<PostRecentPlays> prop) {
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

            
            return Ok(new ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>> {
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
}
