using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Class;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Lib;

namespace OsuDroid.Controllers.Api2;

public class Api2Leaderboard : ControllerExtensions {
    [HttpPost("/api2/leaderboard")]
    [PrivilegeRoute(route: "/api2/leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaderBoard([FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoard> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false) {
                return BadRequest();
            }
                

            var allRegion = prop.Body!.IsRegionAll();
            Result<List<ViewLeaderBoardUser>, string> rep;
            switch (allRegion) {
                case true:
                    rep = (await LeaderBoard.AnyRegionAsync(db, prop.Body.Limit)).Map(x => x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());
                    break;
                default: {
                    var countyRep = prop.Body!.GetRegionAsCountry();
                    if (countyRep.IsSet() == false) {
                        await log.AddLogDebugAsync("RegionAsCountry Not Found");
                        return BadRequest();
                    }

                    rep = (await LeaderBoard.FilterRegionAsync(db, prop.Body.Limit, countyRep.Unwrap())).Map(x => x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());
                    break;
                }
            }

            return Ok(rep == EResult.Err
                ? ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.NotExist()
                : ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.Exist(rep.Ok()));
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

    [HttpPost("/api2/leaderboard/user")]
    [PrivilegeRoute(route: "/api2/leaderboard/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank([FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest();

            var rep = ((await log.AddResultAndTransformAsync(
                await LeaderBoard.UserAsync(db, prop.Body!.UserId))).OkOr(Option<LeaderBoardUser>.Empty))
                .Map(ViewLeaderBoardUser.FromLeaderBoardUser);

            return Ok(rep.IsSet() == false
                ? ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>.NotExist()
                : ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>.Exist(rep.Unwrap()));
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

    [HttpPost("/api2/leaderboard/search-user")]
    [PrivilegeRoute(route: "/api2/leaderboard/search-user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank([FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest();

            ((ILogRequestJsonPrint)prop.Body!).LogRequestJsonPrint();



            ResultOk<List<ViewLeaderBoardUser>> rep = (prop.Body!.IsRegionAll() switch {
                true => await log.AddResultAndTransformAsync(await LeaderBoard.SearchUserAsync(db, prop.Body!.Limit, prop.Body!.Query!)),
                _ => await log.AddResultAndTransformAsync(await LeaderBoard.SearchUserWithRegionAsync(db, prop.Body!.Limit, prop.Body!.Query!,
                    prop.Body.GetRegionAsCountry().Unwrap()))
            }).Map(x => x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());

            return Ok(rep == EResult.Err
                ? ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.NotExist()
                : ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.Exist(rep.OkOr(new())));
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
