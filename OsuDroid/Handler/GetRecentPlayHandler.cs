using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class GetRecentPlayHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<RecentPlaysDto>,
    OptionHandlerOutput<IReadOnlyList<ViewPlayScoreWithUsername>>> {
    public async ValueTask<Result<OptionHandlerOutput<IReadOnlyList<ViewPlayScoreWithUsername>>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<RecentPlaysDto> request) {
        var log = logger.Logger;
        var db = dbWrapper.Db;
        var prop = request.Post;

        var result = (await Query.PlayRecentFilterByAsync(
            db,
            prop.FilterPlays,
            prop.OrderBy,
            prop.Limit,
            prop.StartAt)).Map(x => x.Select(ViewPlayScoreWithUsername.FromPlayScoreWithUsername).ToList());

        if (result == EResult.Err)
            return result.ChangeOkType<OptionHandlerOutput<IReadOnlyList<ViewPlayScoreWithUsername>>>();

        return Result<OptionHandlerOutput<IReadOnlyList<ViewPlayScoreWithUsername>>, string>
            .Ok(OptionHandlerOutput<IReadOnlyList<ViewPlayScoreWithUsername>>
                .With(result.Ok()));
    }
}