using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Model;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Handler;

public class GetLeaderBoardHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<LeaderBoardDto>,
        OptionHandlerOutput<List<ViewLeaderBoardUser>>> {
    public async ValueTask<Result<OptionHandlerOutput<List<ViewLeaderBoardUser>>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<LeaderBoardDto> request) {
        var leaderBoard = request.Post;
        var allRegion = leaderBoard.IsRegionAll();
        var db = dbWrapper.Db;

        Result<List<ViewLeaderBoardUser>, string> rep;

        switch (allRegion) {
            case true:
                rep = (await LeaderBoard.AnyRegionAsync(db, leaderBoard.Limit)).Map(x =>
                    x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());
                break;
            default: {
                var countyRep = leaderBoard.GetRegionAsCountry();
                if (countyRep.IsSet() == false)
                    return Result<OptionHandlerOutput<List<ViewLeaderBoardUser>>, string>
                        .Ok(OptionHandlerOutput<List<ViewLeaderBoardUser>>.Empty);

                rep = (await LeaderBoard.FilterRegionAsync(db, leaderBoard.Limit, countyRep.Unwrap())).Map(x =>
                    x.Select(ViewLeaderBoardUser.FromLeaderBoardUser).ToList());
                break;
            }
        }

        if (rep == EResult.Err) return rep.ChangeOkType<OptionHandlerOutput<List<ViewLeaderBoardUser>>>();

        return Result<OptionHandlerOutput<List<ViewLeaderBoardUser>>, string>
            .Ok(OptionHandlerOutput<List<ViewLeaderBoardUser>>.With(rep.Ok()));
    }
}