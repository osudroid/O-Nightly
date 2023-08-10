using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Extension;

namespace OsuDroid.Handler;

public class WebProfileTopPlaysPageHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<TopPlaysPageingDto>,
        OptionHandlerOutput<ViewPlays>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewPlays>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerGetWrapper<TopPlaysPageingDto> request) {
        var db = dbWrapper.Db;
        var userId = request.Get.UserId;
        var page = request.Get.Page;
        var pageSize = 50;

        var sql = @$"
SELECT *
FROM (
         SELECT distinct ON (filename) * FROM PlayScore
         WHERE UserId = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score
LIMIT {pageSize}
OFFSET {page * pageSize}
;";

        var res = (await db.SafeQueryAsync<Entities.PlayScore>(sql)).Map(x => OptionHandlerOutput<ViewPlays>.With(
                new ViewPlays {
                    Found = true,
                    Scores = x.Select(ViewPlayScore.FromPlayScore).ToList()
                }
            )
        );

        return res;
    }
}