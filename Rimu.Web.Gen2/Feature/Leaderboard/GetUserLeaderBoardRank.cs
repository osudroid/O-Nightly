using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Leaderboard;

public class GetUserLeaderBoardRank: FastEndpoints.Endpoint<
    GetUserLeaderBoardRank.UserLeaderBoardRankRequest, 
    Results<Ok<List<LeaderBoardUserDto>>, BadRequest<string>, InternalServerError, NotFound>
> {
    private readonly IQueryUserStats _queryUserStats;
    private readonly ICountryInfoProvider _countryInfoProvider;

    public GetUserLeaderBoardRank(IQueryUserStats queryUserStats, ICountryInfoProvider countryInfoProvider) {
        _queryUserStats = queryUserStats;
        _countryInfoProvider = countryInfoProvider;
    }

    public override void Configure() {
        Get("/api2/leaderboard/user");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<List<LeaderBoardUserDto>>, BadRequest<string>, InternalServerError, NotFound>> ExecuteAsync(
        UserLeaderBoardRankRequest request,
        CancellationToken ct) {
        
        var result = await _queryUserStats.LeaderBoardUserRank(request.UserId);
        if (result == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        var userOpt = result.Ok();
        if (userOpt.IsNotSet()) {
            return TypedResults.Ok(new List<LeaderBoardUserDto>());
        }

        return TypedResults.Ok(new List<LeaderBoardUserDto>() { LeaderBoardUserDto.FromLeaderBoardUser(userOpt.Unwrap()) });
    }


    public class UserLeaderBoardRankRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "UserId")]
        public long UserId { get; set; }
    }

    public class UserLeaderBoardRankValidator : Validator<UserLeaderBoardRankRequest> {
        public UserLeaderBoardRankValidator() {
            RuleFor(x => x.UserId)
                .GreaterThan(-1);
        }
    }
}