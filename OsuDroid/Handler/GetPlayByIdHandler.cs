using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Model;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Handler; 

public class GetPlayByIdHandler 
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<Api2PlayByIdDto>, OptionHandlerOutput<ViewPlayInfoById>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewPlayInfoById>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<Api2PlayByIdDto> request) {

        var log = logger.Logger;
        var db = dbWrapper.Db;
        var prop = request.Post;
        
        await log.AddLogDebugAsync("PlayId: " + prop.PlayId);
        var optionRep = (await log.AddResultAndTransformAsync(await ScorePack
                .GetByPlayIdAsync(db, prop.PlayId)))
            .OkOr(Option<(Entities.PlayScore Score, string Username, string Region)>.Empty);

        await (optionRep.IsSet()
            ? log.AddLogDebugAsync("PlayId Found")
            : log.AddLogDebugAsync("PlayId Not Found"));

        if (optionRep.IsNotSet()) {
            return Result<OptionHandlerOutput<ViewPlayInfoById>, string>.Ok(OptionHandlerOutput<ViewPlayInfoById>.Empty);
        }
        
        return Result<OptionHandlerOutput<ViewPlayInfoById>, string>
            .Ok(OptionHandlerOutput<ViewPlayInfoById>
            .With(new ViewPlayInfoById {
            Region = optionRep.Unwrap().Region,
            Score = ViewPlayScore.FromPlayScore(optionRep.Unwrap().Score),
            Username = optionRep.Unwrap().Username
        }));
    }
}