using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public class GetProfileTimeline: FastEndpoints.Endpoint<GetProfileTimeline.ProfileTimelineRequestDto,
    Results<Ok<GetProfileTimeline.ProfileTimelineDto>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryGlobalRankingTimeline _queryGlobalRankingTimeline;

    public GetProfileTimeline( 
        IQueryGlobalRankingTimeline queryGlobalRankingTimeline) {
        
        _queryGlobalRankingTimeline = queryGlobalRankingTimeline;
    }

    public override void Configure() {
        this.Get("/api2/profile/timeline/{UserId:long}");
        this.AllowAnonymous();
    }
    
    public override async Task<Results<Ok<ProfileTimelineDto>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileTimelineRequestDto req, CancellationToken ct) {
        
        var dateOnly = DateOnly.FromDateTime(DateTime.Today.AddDays(req.Limit * -1));
        
        var globalRankingTimelineResult = await _queryGlobalRankingTimeline
            .GetRangeByUserIdAndStartAtAsync(req.UserId, dateOnly);
        
        if (globalRankingTimelineResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        var globalRankingTimeline = globalRankingTimelineResult
                                    .Ok()
                                    .Select(ProfileTimelineDto.ProfileTimelinePoint.From)
                                    .ToArray()
        ;

        Array.Sort(globalRankingTimeline);
        if (req.OrderBy == "DESC") {
            Array.Reverse(globalRankingTimeline);
        }

        return TypedResults.Ok(new ProfileTimelineDto() {
            UserId = req.UserId,
            TimeOrder = req.OrderBy,
            ProfileTimelinePoints = globalRankingTimeline
        });
    }

    public sealed class ProfileTimelineRequestDto {
        [FromRoute(Name = "UserId")]  public long UserId { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "limit")]   public long Limit { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "orderBy")] public string OrderBy { get; set; } = "";
        
        public class ProfileTimelineRequestDtoValidation: Validator<ProfileTimelineRequestDto> {
            public ProfileTimelineRequestDtoValidation() {
                RuleFor(x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                ;
                RuleFor(x => x.Limit)
                    .GreaterThanOrEqualTo(0)
                    .LessThan(366)
                ;
                
                RuleFor(x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                ;
            }
        }
    }

    public class ProfileTimelineDto {
        public required long UserId { get; set; }
        public required string TimeOrder { get; set; }
        public required ProfileTimelinePoint[] ProfileTimelinePoints { get; set; }
        
        public class ProfileTimelinePoint: IComparable<ProfileTimelinePoint> {
            public required DateOnly Date { get; init; }
            public required long GlobalRanking { get; init; }
            public required double Pp { get; init; }


            public int CompareTo(ProfileTimelinePoint? other) {
                if (ReferenceEquals(this, other)) return 0;
                if (other is null) return 1;
                return Date.CompareTo(other.Date);
            }

            public static ProfileTimelinePoint From(GlobalRankingTimeline globalRankingTimeline) {
                return new ProfileTimelinePoint {
                    Date = DateOnly.FromDateTime(globalRankingTimeline.Date),
                    GlobalRanking = globalRankingTimeline.GlobalRanking,
                    Pp = globalRankingTimeline.Pp,
                };
            }
        }
    }
}