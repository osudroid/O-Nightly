using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class GetProfileRecentPlays: FastEndpoints.Endpoint<GetProfileTopPlays.ProfileTopPlaysRequest,
    Results<Ok<PlayPlayStatsDto[]>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;

    public GetProfileRecentPlays(IQueryView_Play_PlayStats queryViewPlayPlayStats) {
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
    }

    public override void Configure() {
        Get("/api2/profile/recentplays/{UserId:long}");
        this.AllowAnonymous();
    }
    
    public override async Task<Results<Ok<PlayPlayStatsDto[]>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(GetProfileTopPlays.ProfileTopPlaysRequest req, CancellationToken ct) {

        var result = (await _queryView_Play_PlayStats.GetLastPlayScoreFilterByUserIdAsync(req.UserId, req.Limit))
            .Map(static x => x
                             .Select(static x => x.ToDto())
                             .ToArray()
            )
            ;
        if (result == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok(result.Ok());
    }

    public sealed class ProfileRecentPlaysRequest {
        [FromRoute(Name = "UserId")]
        public long UserId { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "limit")]

        public int Limit { get; set; }
        
        public class ProfileRecentPlaysRequestValidation: Validator<ProfileRecentPlaysRequest> {
            public ProfileRecentPlaysRequestValidation() {
                RuleFor(x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                ;
                RuleFor(x => x.Limit)
                    .GreaterThanOrEqualTo(1)
                    .LessThanOrEqualTo(100)
                ;
            }
        }
    }
}