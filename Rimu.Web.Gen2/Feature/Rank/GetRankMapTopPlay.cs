using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.Feature.Profile;
using Rimu.Web.Gen2.Share.Rank;

namespace Rimu.Web.Gen2.Feature.Rank;

public sealed class GetRankMapTopPlay: FastEndpoints.Endpoint<GetRankMapTopPlay.RankMapTopPlayRequest,
    Results<Ok<MapRankDto[]>, BadRequest, InternalServerError, NotFound>
>  {
    private readonly IQueryView_Play_PlayStats_UserInfo _queryView_Play_PlayStats_UserInfo;

    public GetRankMapTopPlay(IQueryView_Play_PlayStats_UserInfo queryViewPlayPlayStatsUserInfo) {
        _queryView_Play_PlayStats_UserInfo = queryViewPlayPlayStatsUserInfo;
    }

    public override void Configure() {
        Get("/api2/rank/map");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<MapRankDto[]>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(RankMapTopPlayRequest req, CancellationToken ct) {

        var mapRankDtosResult = (await _queryView_Play_PlayStats_UserInfo.BeatmapTopAsync(req.Filename, req.Filehash, req.Offset, req.Limit))
            .Map(x => 
                x.Select((v, i) => new MapRankDto() {
                    Rank = x.Rank + req.Offset, Stat = View_Play_PlayStats_Dto.From(v), Username = v.Username??"",
                }).ToArray());

        if (mapRankDtosResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok(mapRankDtosResult.Ok());
    }
    
    public sealed class RankMapTopPlayRequest {
        public string Filehash { get; set; } = "";
        public string Filename { get; set; } = "";
        public long Offset { get; set; }
        public int Limit { get; set; }

        public sealed class RankMapTopPlayRequestValidator : Validator<RankMapTopPlayRequest> {
            public RankMapTopPlayRequestValidator() {
                RuleFor(x => x.Filehash)
                    .Length(32);
                RuleFor(x => x.Filename)
                    .MinimumLength(1)
                    .MaximumLength(512)
                ;
                RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
                RuleFor(x => x.Limit).GreaterThanOrEqualTo(0);
                
            }
        }
    }
}