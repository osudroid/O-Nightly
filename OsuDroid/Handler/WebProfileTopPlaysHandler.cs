using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Extension;

namespace OsuDroid.Handler;

public class WebProfileTopPlaysHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<UserIdBoxDto>,
        OptionHandlerOutput<ViewPlays>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewPlays>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerGetWrapper<UserIdBoxDto> request) {
        var db = dbWrapper.Db;
        var userId = request.Get.UserId;

        var res = (await db.SafeQueryAsync<Entities.PlayScore>(@$"
SELECT * 
FROM (
         SELECT distinct ON (filename) * FROM PlayScore
         WHERE Userid = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score DESC 
LIMIT 50;
"
        )).Map(x => OptionHandlerOutput<ViewPlays>.With(new ViewPlays {
                    Found = true,
                    Scores = x.Select(ViewPlayScore.FromPlayScore).ToList()
                }
            )
        );

        return res;
    }
}