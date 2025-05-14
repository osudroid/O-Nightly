using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class GetProfileStatsMarksCount: FastEndpoints.Endpoint<GetProfileStatsMarksCount.ProfileStatsMarksCountRequest,
    Results<Ok<GetProfileStatsMarksCount.OverallStatsDto>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryUserStats _queryUserStats;

    public GetProfileStatsMarksCount(IQueryUserStats queryUserStats) {
        _queryUserStats = queryUserStats;
    }

    public override void Configure() {
        Get("/api2/profile/stats/marks-count");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<OverallStatsDto>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileStatsMarksCountRequest req, CancellationToken ct) {

        var userStatsResult = await _queryUserStats.GetByUserIdAsync(req.UserId);
        if (userStatsResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (userStatsResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }

        UserStats userStats = userStatsResult.Ok().Unwrap();
        
        return TypedResults.Ok(OverallStatsDto.From(userStats));
    }

    public sealed class OverallStatsDto {
        public required long OverallPlaycount { get; set; }
        public required long OverallScore { get; set; }
        public required double OverallAccuracy { get; set; }
        public required long OverallCombo { get; set; }
        public required long OverallXss { get; set; }
        public required long OverallSs { get; set; }
        public required long OverallXs { get; set; }
        public required long OverallS { get; set; }
        public required long OverallA { get; set; }
        public required long OverallB { get; set; }
        public required long OverallC { get; set; }
        public required long OverallD { get; set; }
        public required long OverallPerfect { get; set; }
        public required long OverallHits { get; set; }
        public required long Overall300 { get; set; }
        public required long Overall100 { get; set; }
        public required long Overall50 { get; set; }
        public required long OverallGeki { get; set; }
        public required long OverallKatu { get; set; }
        public required long OverallMiss { get; set; }
        public required double OverallPp { get; set; }

        public static OverallStatsDto From(IUserStatsReadonly userStats) {
            return new() {
                OverallPlaycount = userStats.OverallPlaycount,
                OverallScore = userStats.OverallScore,
                OverallAccuracy = userStats.OverallAccuracy,
                OverallCombo = userStats.OverallCombo,
                OverallXss = userStats.OverallXss,
                OverallSs = userStats.OverallSs,
                OverallXs = userStats.OverallXs,
                OverallS = userStats.OverallS,
                OverallA = userStats.OverallA,
                OverallB = userStats.OverallB,
                OverallC = userStats.OverallC,
                OverallD = userStats.OverallD,
                OverallPerfect = userStats.OverallPerfect,
                OverallHits = userStats.OverallHits,
                Overall300 = userStats.Overall300,
                Overall100 = userStats.Overall100,
                Overall50 = userStats.Overall50,
                OverallGeki = userStats.OverallGeki,
                OverallKatu = userStats.OverallKatu,
                OverallMiss = userStats.OverallMiss,
                OverallPp = userStats.OverallPp,
            };
        }
    }
    
    public sealed class ProfileStatsMarksCountRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "UserId")]
        public long UserId { get; set; }
        
        public class ProfileStatsMarksCountRequestValidation: Validator<ProfileStatsMarksCountRequest> {
            public ProfileStatsMarksCountRequestValidation() {
                RuleFor(x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                    ;
            }
        }
    }
}