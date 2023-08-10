using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class WebProfileStatsTimeLineHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<UserIdBoxDto>,
        OptionHandlerOutput<ViewUserRankTimeLine>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewUserRankTimeLine>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerGetWrapper<UserIdBoxDto> request) {
        var db = dbWrapper.Db;
        var userId = request.Get.UserId;

        var result = await QueryGlobalRankingTimeline
            .BuildTimeLineAsync(db, userId, DateTime.UtcNow - TimeSpan.FromDays(90));

        if (result == EResult.Err)
            return result.ChangeOkType<OptionHandlerOutput<ViewUserRankTimeLine>>();

        var rankingTimeline = result
                              .Ok()
                              .Select(x => new ViewUserRankTimeLine.RankTimeLineValue {
                                      Date = x.Date,
                                      Score = x.Score,
                                      Rank = x.GlobalRanking
                                  }
                              )
                              .ToList();

        return Result<OptionHandlerOutput<ViewUserRankTimeLine>, string>
            .Ok(OptionHandlerOutput<ViewUserRankTimeLine>.With(new ViewUserRankTimeLine {
                        UserId = userId,
                        List = rankingTimeline
                    }
                )
            );
    }
}