using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Region.Adapter.Interface;


namespace Rimu.Web.Gen2.Feature.Leaderboard;

public class GetLeaderBoard: FastEndpoints.Endpoint<
    GetLeaderBoard.LeaderBoardRequest, 
    Results<Ok<List<LeaderBoardUserDto>>, BadRequest<string>, InternalServerError, NotFound>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IQueryUserStats _queryUserStats;
    private readonly ICountryInfoProvider _countryInfoProvider;

    public GetLeaderBoard(IQueryUserStats queryUserStats, ICountryInfoProvider countryInfoProvider) {
        _queryUserStats = queryUserStats;
        _countryInfoProvider = countryInfoProvider;
    }

    public override void Configure() {
        Get("/api2/leaderboard");
        AllowAnonymous();
    }


    public override async Task<Results<Ok<List<LeaderBoardUserDto>>, BadRequest<string>, InternalServerError, NotFound>> ExecuteAsync(LeaderBoardRequest req, CancellationToken ct) {
        var allRegion = req.IsRegionAll();

        ResultOk<List<LeaderBoardUserDto>> rep;

        switch (allRegion) {
            case true:
                rep = (await _queryUserStats.LeaderBoardNoFilter(req.Limit))
                    .Map(x => x.Select(LeaderBoardUserDto.FromLeaderBoardUser).ToList());
                break;
            default: {
                var countyRep = _countryInfoProvider.GetRegionAsCountry(req.Region);
                
                if (countyRep.IsSet() == false) {
                    return TypedResults.Ok(new List<LeaderBoardUserDto>());
                }


                rep = (await _queryUserStats.LeaderBoardFilterCountry(
                        req.Limit, countyRep.Unwrap().NameShort.ToUpper()
                    ))
                    .Map(x => x.Select(LeaderBoardUserDto.FromLeaderBoardUser))
                    .Map(x => x.ToList())
                ;
                break;
            }
        }

        if (rep == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(rep.Ok());
    }
    
    

    public class LeaderBoardRequest {
        private string _region = "";
    
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "Region")]
        public string Region {
            get => _region;
            set => _region = value == "All" || value == "" ? "all" : value;
        }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "Limit")]
        public int Limit { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "offset")]
        public int Offset { get; set; }
        
        public bool IsRegionAll() => Region == "All";
    }

    public class LeaderBoardValidation : Validator<LeaderBoardRequest> {
        public LeaderBoardValidation() {
            RuleFor(x => x.Region).MinimumLength(2);
            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
            RuleFor(x => x.Offset).GreaterThan(-1);
        }
    }
}