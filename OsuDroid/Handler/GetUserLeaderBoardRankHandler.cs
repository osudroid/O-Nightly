using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler; 

public class GetUserLeaderBoardRankHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<LeaderBoardUserDto>, OptionHandlerOutput<ViewLeaderBoardUser>> {
    
    public async ValueTask<Result<OptionHandlerOutput<ViewLeaderBoardUser>, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<LeaderBoardUserDto> request) {
        var result = await QueryUserStats.LeaderBoardUserRank(dbWrapper.Db, request.Post.UserId);
        if (result == EResult.Err) {
            return result.ChangeOkType<OptionHandlerOutput<ViewLeaderBoardUser>>();
        }

        var userOpt = result.Ok();
        if (userOpt.IsNotSet()) {
            return Result<OptionHandlerOutput<ViewLeaderBoardUser>, string>
                .Ok(OptionHandlerOutput<ViewLeaderBoardUser>.Empty);
        }

        var user = userOpt.Unwrap();
        return Result<OptionHandlerOutput<ViewLeaderBoardUser>, string>
            .Ok(OptionHandlerOutput<ViewLeaderBoardUser>.With(ViewLeaderBoardUser.FromLeaderBoardUser(user)));
    }
}