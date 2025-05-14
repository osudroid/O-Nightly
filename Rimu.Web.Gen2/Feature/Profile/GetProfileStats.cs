using FluentValidation;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public class GetProfileStats: FastEndpoints.Endpoint<GetProfileStats.UserIdRequestDto,
    Results<Ok<ProfileStatsDto>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryUserSetting _queryUserSetting;
    private readonly IQueryUserStats _queryUserStats;
    private readonly IQueryUserClassifications _queryUserClassifications;

    public GetProfileStats(IQueryUserInfo queryUserInfo, IQueryUserSetting queryUserSetting, IQueryUserStats queryUserStats, IQueryUserClassifications queryUserClassifications) {
        _queryUserInfo = queryUserInfo;
        _queryUserSetting = queryUserSetting;
        _queryUserStats = queryUserStats;
        _queryUserClassifications = queryUserClassifications;
    }

    public override void Configure() {
        this.Get("/api2/profile/stats/{UserId:long}");
        this.AllowAnonymous();
    }
    
    public override async Task<Results<Ok<ProfileStatsDto>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(UserIdRequestDto req, CancellationToken ct) {

        var userInfoResult =  await _queryUserInfo.GetByUserIdAsync(req.UserId);
        var userSettingResult =  await _queryUserSetting.GetByUserIdAsync(req.UserId);
        var userStatsResult =  await _queryUserStats.GetByUserIdAsync(req.UserId);
        var userClassificationsResult =  await _queryUserClassifications.GetByUserIdAsync(req.UserId);
        var globalUserRankResult =  await _queryUserStats.GetUserRank(req.UserId);

        if (userInfoResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (userInfoResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }
        
        var countryUserRankResult =  await _queryUserStats.UserRankByUserIdScopeCountryAsync(req.UserId, userInfoResult.Ok().Unwrap().Region??"");
        
        if (countryUserRankResult.OkOrDefault().IsNotSet()
            || userSettingResult.OkOrDefault().IsNotSet()
            || userStatsResult.OkOrDefault().IsNotSet()
            || userClassificationsResult.OkOrDefault().IsNotSet()
            || globalUserRankResult == EResult.Err) {
            
            return TypedResults.InternalServerError();
        }
        
        var userInfo = userInfoResult.Ok().Unwrap();
        var userSetting = userSettingResult.Ok().Unwrap();
        var userStats = userStatsResult.Ok().Unwrap();
        var userClassifications = userClassificationsResult.Ok().Unwrap();
        var countryUserRank = countryUserRankResult.Ok().Unwrap(); 
        var globalUserRank = globalUserRankResult.Ok(); 
        
        return TypedResults.Ok(ProfileStatsDto.FromProfile(
            userInfo: userInfo, 
            userStats: userStats, 
            userClassifications: Option<IUserClassificationsReadonly>.NullSplit(userSetting.ShowUserClassifications? userClassifications: null), 
            globalRanking: globalUserRank,
            countryRanking: countryUserRank
        ));
    }



    public sealed class UserIdRequestDto {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "UserId")]
        public long UserId { get; set; }
        
        public class UserIdRequestDtoValidation: Validator<UserIdRequestDto> {
            public UserIdRequestDtoValidation() {
                RuleFor(x => x.UserId)
                    .GreaterThanOrEqualTo(0)
                ;
            }
        }
    }
}