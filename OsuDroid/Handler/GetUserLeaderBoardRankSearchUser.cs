using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Model;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Handler;

public class GetUserLeaderBoardRankSearchUser
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<LeaderBoardSearchUserDto>,
        OptionHandlerOutput<List<ViewLeaderBoardUser>>> {
    public async ValueTask<Result<OptionHandlerOutput<List<ViewLeaderBoardUser>>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<LeaderBoardSearchUserDto> request) {
        var db = dbWrapper.Db;
        var log = logger.Logger;
        var search = request.Post;
        var leaderBoardSearch = search;

        var rep = (leaderBoardSearch.IsRegionAll() switch {
            true => await log.AddResultAndTransformAsync(await LeaderBoard
                .SearchUserAsync(db, search.Limit, search.Query)),
            _ => await log.AddResultAndTransformAsync(await LeaderBoard
                .SearchUserWithRegionAsync(db, search.Limit, search.Query,
                    search.GetRegionAsCountry().Unwrap()))
        }).Map(x => x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());

        if (rep == EResult.Err)
            return Result<OptionHandlerOutput<List<ViewLeaderBoardUser>>, string>.Ok(
                OptionHandlerOutput<List<ViewLeaderBoardUser>>.Empty);
        return Result<OptionHandlerOutput<List<ViewLeaderBoardUser>>, string>.Ok(
            OptionHandlerOutput<List<ViewLeaderBoardUser>>
                .With(rep.Ok()));
    }
}