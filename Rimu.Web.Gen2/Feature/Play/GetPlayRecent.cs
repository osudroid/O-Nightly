using FastEndpoints;
using FluentValidation;
using LamLibAllOver;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Play;

public sealed class GetPlayRecent: FastEndpoints.Endpoint<GetPlayRecent.PlayRecentRequest,
    Results<Ok<PlayPlayScoreWithUsernameDto[]>, BadRequest, InternalServerError>
> {
    private readonly IQuery _query;

    public GetPlayRecent(IQuery query) {
        _query = query;
    }

    public override void Configure() {
        Get("/api2/play/recent");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<PlayPlayScoreWithUsernameDto[]>, BadRequest, InternalServerError>> 
        ExecuteAsync(PlayRecentRequest req, CancellationToken ct) {
        
        var queryResult = await _query.PlayRecentFilterByAsync(
            filterPlays: req.FilterPlays,
            orderBy: req.OrderBy,
            limit: req.Limit,
            startAt: req.StartAt
        );
        
        if (queryResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (queryResult.Ok().Count == 0) {
            return TypedResults.Ok<PlayPlayScoreWithUsernameDto[]>([]);
        }
        
        return TypedResults
            .Ok<PlayPlayScoreWithUsernameDto[]>(
                queryResult
                    .Ok()
                    .Select(x => 
                        new PlayPlayScoreWithUsernameDto() { 
                            PlayPlayStatsDto = PlayPlayStatsDto.CreateWith(x), 
                            Username = x.Username??""
                        })
                    .ToArray()
            );
    }

    public sealed class PlayRecentRequest {
        public string FilterPlays { get; set; } = "";
        public string OrderBy { get; set; } = "";
        public int Limit { get; set; }
        public int StartAt { get; set; }
        
        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] {
                    FilterPlays ?? "",
                    OrderBy ?? "",
                    Limit,
                    StartAt
                }
            );
        }

        public sealed class PlayRecentRequestValidator : Validator<PlayRecentRequest> {
            public PlayRecentRequestValidator() {
                RuleFor(static x => x.FilterPlays)
                    .Must(static v => {
                        return v switch {
                            "Any"
                                or "XSS_Plays"
                                or "SS_Plays"
                                or "XS_Plays"
                                or "S_Plays"
                                or "A_Plays"
                                or "B_Plays"
                                or "C_Plays"
                                or "D_Plays"
                                or "Accuracy_100"
                                => true,
                            _ => false
                        };
                    }
                );
                

                RuleFor(static x => x.OrderBy)
                    .Must(static v => {
                            return v switch {
                                "Time_ASC"
                                    or "Time_DESC"
                                    or "Score_ASC"
                                    or "Score_DESC"
                                    or "Combo_ASC"
                                    or "Combo_DESC"
                                    or "50_ASC"
                                    or "50_DESC"
                                    or "100_ASC"
                                    or "100_DESC"
                                    or "300_ASC"
                                    or "300_DESC"
                                    or "Miss_ASC"
                                    or "Miss_DESC"
                                    => true,
                                _ => false
                            };
                        }
                    );

                RuleFor(static x => x.Limit)
                    .GreaterThanOrEqualTo(1)
                    .LessThanOrEqualTo(100)
                ;

                RuleFor(static x => x.StartAt)
                    .GreaterThanOrEqualTo(0);
            }
        }
    }
}