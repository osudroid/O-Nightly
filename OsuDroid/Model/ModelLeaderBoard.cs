using LamLogger;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.View;

namespace OsuDroid.Model;

public static class ModelLeaderBoard {
    public static async Task<Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>, string>>
        GetLeaderBoardAsync(ControllerExtensions controller, NpgsqlConnection db, LeaderBoardDto leaderBoard) {
        var allRegion = leaderBoard.IsRegionAll();
        Result<List<ViewLeaderBoardUser>, string> rep;

        switch (allRegion) {
            case true:
                rep = (await LeaderBoard.AnyRegionAsync(db, leaderBoard.Limit)).Map(x =>
                    x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());
                break;
            default: {
                var countyRep = leaderBoard.GetRegionAsCountry();
                if (countyRep.IsSet() == false)
                    return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>, string>
                        .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>.BadRequest());

                rep = (await LeaderBoard.FilterRegionAsync(db, leaderBoard.Limit, countyRep.Unwrap())).Map(x =>
                    x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());
                break;
            }
        }

        return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>, string>
            .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>
                .Ok(rep == EResult.Err
                    ? ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.NotExist()
                    : ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.Exist(rep.Ok())));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>, string>>
        GetUserLeaderBoardRank(
            ControllerExtensions controller,
            NpgsqlConnection db,
            LamLog log,
            LeaderBoardSearchUserDto leaderBoardSearch) {
        var search = leaderBoardSearch;

        var rep = (leaderBoardSearch.IsRegionAll() switch {
            true => await log.AddResultAndTransformAsync(await LeaderBoard
                .SearchUserAsync(db, search.Limit, search.Query)),
            _ => await log.AddResultAndTransformAsync(await LeaderBoard
                .SearchUserWithRegionAsync(db, search.Limit, search.Query,
                    search.GetRegionAsCountry().Unwrap()))
        }).Map(x => x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());

        return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>, string>
            .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>
                .Ok(rep == EResult.Err
                    ? ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.NotExist()
                    : ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>.Exist(
                        rep.OkOr(new List<ViewLeaderBoardUser>()))));
    }
}