using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class GetProfileTopPlayByMark: FastEndpoints.Endpoint<GetProfileTopPlayByMark.ProfileTopPlayByMarkRequest,
    Results<Ok<PlayPlayStatsDto[]>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryView_Play_PlayStats _queryView_Play_Play_Stats;

    public GetProfileTopPlayByMark(IQueryView_Play_PlayStats queryViewPlayPlayStats) {
        _queryView_Play_Play_Stats = queryViewPlayPlayStats;
    }

    public override void Configure() {
        Get("/api2/profile/topplay/by-mark");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<PlayPlayStatsDto[]>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileTopPlayByMarkRequest req, CancellationToken ct) {

        if (!Enum.TryParse<PlayStatsDto.EPlayScoreMark>(req.Mark, out var mark)) {
            return TypedResults.BadRequest();
        }

        var viewPlayPlayStatsResult = await _queryView_Play_Play_Stats
            .GetTopScoreFromUserIdFilterMarkAsync(req.UserId, req.Limit, req.Offset, mark);

        if (viewPlayPlayStatsResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok(viewPlayPlayStatsResult.Ok().Select(static x => x.ToDto()).ToArray());
    }
    
    public sealed class ProfileTopPlayByMarkRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "userId")] public long UserId { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "limit")]  public int Limit { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "offset")] public long Offset { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "mark")] public string Mark { get; set; } = "";
        
        public class ProfileTopPlayByMarkRequestValidation: Validator<ProfileTopPlayByMarkRequest> {
            public ProfileTopPlayByMarkRequestValidation() {
                RuleFor(static x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                    ;
                RuleFor(static x => x.Offset)
                    .GreaterThanOrEqualTo(0)
                    ;
                RuleFor(static x => x.Limit)
                    .GreaterThanOrEqualTo(1)
                    .LessThanOrEqualTo(100)
                    ;
                RuleFor(static x => x.Mark)
                    .Must(static x => Enum.TryParse<PlayStatsDto.EPlayScoreMark>(x, out _))
                    ;
            }
        }
    }
}