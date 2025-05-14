using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class ProfileStatsDto {
    public required long UserId { get; set; }
    public required string? Username { get; set; }
    public required string? Region { get; set; }
    public required long GlobalRanking { get; set; }
    public required long CountryRanking { get; set; }

    public UserClassificationsDto? UserClassifications { get; set; }
    public UserStatsDto? UserStats { get; set; }
    
    public static ProfileStatsDto FromProfile(
        IUserInfoReadonly userInfo, 
        IUserStatsReadonly userStats, 
        Option<IUserClassificationsReadonly> userClassifications,
        long globalRanking, 
        long countryRanking) {
        
        return new ProfileStatsDto() {
            UserId = userStats.UserId,
            Username = userInfo.Username,
            Region = userInfo.Region,
            GlobalRanking = globalRanking,
            CountryRanking = countryRanking, 
            UserStats = UserStatsDto.From(userStats),
            UserClassifications = userClassifications.IsNotSet()
                ? null
                : UserClassificationsDto.From(userClassifications.Unwrap()),
        };
    }
}