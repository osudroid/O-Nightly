using LamLogger;
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
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaderBoard([FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoard> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false) {
                return await RollbackAndGetBadRequestAsync(dbT);
            }


            var result = await log.AddResultAndTransformAsync(await ModelLeaderBoard.GetLeaderBoardAsync(
                this, db, DtoMapper.LeaderBoardToDto(prop.Body!)));

            if (result == EResult.Err) {
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

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

    [HttpPost("/api2/leaderboard/user")]
    [PrivilegeRoute(route: "/api2/leaderboard/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        // TODO Hier weiter

        try {
            if (prop.ValuesAreGood() == false)
                return await RollbackAndGetBadRequestAsync(dbT);

            var rep = ((await log.AddResultAndTransformAsync(
                    await LeaderBoard.UserAsync(db, prop.Body!.UserId))).OkOr(Option<LeaderBoardUser>.Empty))
                .Map(ViewLeaderBoardUser.FromLeaderBoardUser);

            return Ok(rep.IsSet() == false
                ? ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>.NotExist()
                : ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>.Exist(rep.Unwrap()));
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api2/leaderboard/search-user")]
    [PrivilegeRoute(route: "/api2/leaderboard/search-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return await RollbackAndGetBadRequestAsync(dbT);

            ((ILogRequestJsonPrint)prop.Body!).LogRequestJsonPrint();

            var result = await log.AddResultAndTransformAsync(await ModelLeaderBoard.GetUserLeaderBoardRank(
                this, db, log, DtoMapper.LeaderBoardSearchUserToDto(prop.Body!)));

            if (result == EResult.Err) {
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

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
}