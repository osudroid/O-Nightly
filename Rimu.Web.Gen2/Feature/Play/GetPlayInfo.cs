using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.Feature.Odr;

namespace Rimu.Web.Gen2.Feature.Play;

public sealed class GetPlayInfo: FastEndpoints.Endpoint<GetPlayInfo.PlayInfoRequest,
    Results<Ok<PlayPlayScoreWithUsernameDto>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;
    private readonly IQueryUserInfo _queryUserInfo;

    public GetPlayInfo(IQueryView_Play_PlayStats queryViewPlayPlayStats, IQueryUserInfo queryUserInfo) {
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
        _queryUserInfo = queryUserInfo;
    }

    public override void Configure() {
        this.Get("/api2/play/{playId:long}");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<PlayPlayScoreWithUsernameDto>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(PlayInfoRequest req, CancellationToken ct) {

        var view_Play_PlayStatsResult = await _queryView_Play_PlayStats.GetByIdAsync(req.PlayId);
        if (view_Play_PlayStatsResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (view_Play_PlayStatsResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }

        var view_Play_PlayStats = view_Play_PlayStatsResult.Ok().Unwrap();
        var userInfoResult = await _queryUserInfo.GetByUserIdAsync(view_Play_PlayStats.UserId);
        if (userInfoResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (userInfoResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }
        

        return TypedResults.Ok(new PlayPlayScoreWithUsernameDto() {
            PlayPlayStatsDto = view_Play_PlayStats.ToDto(),
            Username = userInfoResult.Ok().Unwrap().Username??""
        });
    }

    public sealed class PlayInfoRequest {
        [FromRoute(Name = "playId")]
        public long PlayId { get; set; }
    }
}