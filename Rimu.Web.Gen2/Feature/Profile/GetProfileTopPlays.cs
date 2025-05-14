using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class GetProfileTopPlays: FastEndpoints.Endpoint<GetProfileTopPlays.ProfileTopPlaysRequest,
    Results<Ok<PlayPlayStatsDto[]>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryView_Play_PlayStats _queryView_Play_Stats;

    public GetProfileTopPlays(IQueryView_Play_PlayStats queryViewPlayStats) {
        _queryView_Play_Stats = queryViewPlayStats;
    }

    public override void Configure() {
        Get("/api2/profile/topplays");
        this.AllowAnonymous();
    }


    public override async Task<Results<Ok<PlayPlayStatsDto[]>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileTopPlaysRequest req, CancellationToken ct) {

        var result = (await _queryView_Play_Stats.GetTopScoreFromUserIdAsync(req.UserId, req.Limit, req.Offset))
            .Map(static x => x
                             .Select(static x => x.ToDto())
                             .ToArray()
            );

        if (result == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok(result.Ok());
    }


    public sealed class ProfileTopPlaysRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "userId")] public long UserId { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "limit")] public int Limit { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "offset")] public long Offset { get; set; }
        
        public class ProfileTopPlaysRequestValidation: Validator<ProfileTopPlaysRequest> {
            public ProfileTopPlaysRequestValidation() {
                RuleFor(x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                ;
                RuleFor(x => x.Offset)
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