using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Odr;

public class GetOdrZipRedirect: FastEndpoints.Endpoint<OdrZipReplayIdDto,
    Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>
> {
    private readonly IQueryReplayFile _queryReplayFile;
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;
    private readonly IQueryUserInfo _queryUserInfo;

    public GetOdrZipRedirect(
        IQueryReplayFile queryReplayFile, 
        IQueryView_Play_PlayStats queryViewPlayPlayStats, 
        IQueryUserInfo queryUserInfo) {
        
        _queryReplayFile = queryReplayFile;
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
        _queryUserInfo = queryUserInfo;
    }

    public override void Configure() {
        this.Get("/api2/odr/redirect/{replayId:long}.zip");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>> 
        ExecuteAsync(OdrZipReplayIdDto req, CancellationToken ct) {

        var bblScoreResult = (await _queryView_Play_PlayStats.GetByIdAsync(req.ReplayId));
        if (bblScoreResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (bblScoreResult.Ok().IsNotSet()) {
            return TypedResults.BadRequest();
        }
        

        var bblScore = bblScoreResult.Ok().Unwrap();
        var fullname = $"{bblScore.Filename!.Replace(".osu", "")} {bblScore.UserId} {bblScore.Date.Ticks}";

        await this.SendRedirectAsync($"/api2/odr/fullname/{req.ReplayId}/{fullname}.zip", true);
        
        return TypedResults.Ok();
    }
}