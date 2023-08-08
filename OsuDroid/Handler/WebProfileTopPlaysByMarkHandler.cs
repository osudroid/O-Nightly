using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Dto;
using OsuDroidLib.Extension;

namespace OsuDroid.Handler; 

public class WebProfileTopPlaysByMarkHandler : IHandler<NpgsqlCreates.DbWrapper,LogWrapper,ControllerGetWrapper<TopPlaysByMarkPageingDto>,OptionHandlerOutput<ViewPlays>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewPlays>, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerGetWrapper<TopPlaysByMarkPageingDto> request) {
        var db = dbWrapper.Db; 
        var userId = request.Get.UserId;
        var page = request.Get.Page; 
        var pageSize = 50;
        var mark = request.Get.MarkString;
        
        var sql = @$"
SELECT *
FROM (
         SELECT distinct ON (filename) * FROM PLayScore
         WHERE UserId = {userId}
         AND mark = @Mark
         ORDER BY filename, score DESC
     ) x
ORDER BY score
LIMIT {pageSize}
OFFSET {page * pageSize}
;";
        return (await db.SafeQueryAsync<Entities.PlayScore>(sql, new { Mark = mark.ToStringFast() }))
            .Map(x => OptionHandlerOutput<ViewPlays>.With(new ViewPlays() {
                Found = true,
                Scores = x
                         .Select(ViewPlayScore.FromPlayScore)
                         .ToList()
            }));
    }
}