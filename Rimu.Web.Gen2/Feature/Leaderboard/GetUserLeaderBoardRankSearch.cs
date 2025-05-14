using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Leaderboard;

public class GetUserLeaderBoardRankSearch: FastEndpoints.Endpoint<
    GetUserLeaderBoardRankSearch.UserLeaderBoardRankSearchRequest, 
    Results<Ok<List<LeaderBoardUserDto>>, BadRequest<string>, InternalServerError, NotFound>
> {
    private readonly IQueryUserStats _queryUserStats;
    private readonly ICountryInfoProvider _countryInfoProvider;

    public GetUserLeaderBoardRankSearch(IQueryUserStats queryUserStats, ICountryInfoProvider countryInfoProvider) {
        _queryUserStats = queryUserStats;
        _countryInfoProvider = countryInfoProvider;
    }

    public override void Configure() {
        Get("/api2/leaderboard/user/search");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<List<LeaderBoardUserDto>>, BadRequest<string>, InternalServerError, NotFound>> ExecuteAsync(
        UserLeaderBoardRankSearchRequest req, 
        CancellationToken ct) {

        ResultOk<List<LeaderBoardUserDto>> rep = default;
        
        if (req.IsRegion()) {
            rep = (await _queryUserStats.LeaderBoardSearchUser(req.Limit, req.Query))
                  .Map(x => x.Select(LeaderBoardUserDto.FromLeaderBoardUser))
                  .Map(x => x.ToList())    
            ;
        }
        else {
            var countyRep = _countryInfoProvider.GetRegionAsCountry(req.Region);
                
            if (countyRep.IsSet() == false) {
                return TypedResults.Ok(new List<LeaderBoardUserDto>());
            }

            
            rep = (await _queryUserStats.LeaderBoardSearchUser(req.Limit, req.Query,
                    countyRep.Unwrap().NameShort.ToUpper()
                    ))
                    .Map(x => x.Select(LeaderBoardUserDto.FromLeaderBoardUser))
                    .Map(x => x.ToList())
            ;            
        }

        if (rep == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(rep.Ok());
    }

    public class UserLeaderBoardRankSearchRequest {
        private string _region = "all";
        public long Limit { get; set; }
        public string Query { get; set; } = "";

        public string Region {
            get => _region ?? "";
            set => _region = value == "All" || value == "" ? "all" : value;
        }
        
        public bool IsRegion() => Region == "all";
    }
    
    public class UserLeaderBoardRankSearchValidation : Validator<UserLeaderBoardRankSearchRequest> {
        public UserLeaderBoardRankSearchValidation() {
            RuleFor(x => x.Region).MinimumLength(2);
            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
        }
    }
}