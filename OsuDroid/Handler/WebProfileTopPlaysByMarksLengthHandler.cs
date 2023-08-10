using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class WebProfileTopPlaysByMarksLengthHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper,
    ControllerGetWrapper<UserIdBox>, OptionHandlerOutput<ViewPlaysMarksLength>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewPlaysMarksLength>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerGetWrapper<UserIdBox> request) {
        var db = dbWrapper.Db;

        var result = await QueryPlayScore.CountMarkPlaysByUserIdAsync(db, request.Get.UserId);
        if (result == EResult.Err)
            return result.ChangeOkType<OptionHandlerOutput<ViewPlaysMarksLength>>();

        var dic = result.Ok();

        return Result<OptionHandlerOutput<ViewPlaysMarksLength>, string>
            .Ok(OptionHandlerOutput<ViewPlaysMarksLength>
                .With(ViewPlaysMarksLength.Factory(dic))
            );
    }
}