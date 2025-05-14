using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Adapter;

public static class Extension {
    public static IDbContext GetDbContext(this IServiceProvider self) => self.GetService<IDbContext>() ?? throw new InvalidOperationException();
    public static IDbTransactionContext GetDbTransactionContext(this IServiceProvider self) => self.GetService<IDbTransactionContext>() ?? throw new InvalidOperationException();
    
    public static IQuery GetQuery(this IServiceProvider self) => self.GetService<IQuery>() ?? throw new InvalidOperationException();
    public static IQueryGlobalRankingTimeline GetQueryGlobalRankingTimeline(this IServiceProvider self) => self.GetService<IQueryGlobalRankingTimeline>() ?? throw new InvalidOperationException();
    public static IQueryPlay GetQueryPlay(this IServiceProvider self) => self.GetService<IQueryPlay>() ?? throw new InvalidOperationException();
    public static IQueryPlayStats GetQueryPlayStats(this IServiceProvider self) => self.GetService<IQueryPlayStats>() ?? throw new InvalidOperationException();
    public static IQueryPlayStatsHistory GetQueryPlayStatsHistory(this IServiceProvider self) => self.GetService<IQueryPlayStatsHistory>() ?? throw new InvalidOperationException();
    public static IQueryReplayFile GetQueryReplayFile(this IServiceProvider self) => self.GetService<IQueryReplayFile>() ?? throw new InvalidOperationException();
    public static IQueryResetPasswordKey GetQueryResetPasswordKey(this IServiceProvider self) => self.GetService<IQueryResetPasswordKey>() ?? throw new InvalidOperationException();
    public static IQuerySetting GetQuerySetting(this IServiceProvider self) => self.GetService<IQuerySetting>() ?? throw new InvalidOperationException();
    public static IQuerySettingsHot GetQuerySettingsHot(this IServiceProvider self) => self.GetService<IQuerySettingsHot>() ?? throw new InvalidOperationException();
    public static IQueryTokenUser GetQueryTokenUser(this IServiceProvider self) => self.GetService<IQueryTokenUser>() ?? throw new InvalidOperationException();
    public static IQueryUserAvatar GetQueryUserAvatar(this IServiceProvider self) => self.GetService<IQueryUserAvatar>() ?? throw new InvalidOperationException();
    public static IQueryUserInfo GetQueryUserInfo(this IServiceProvider self) => self.GetService<IQueryUserInfo>() ?? throw new InvalidOperationException();
    public static IQueryUserStats GetQueryUserStats(this IServiceProvider self) => self.GetService<IQueryUserStats>() ?? throw new InvalidOperationException();
    public static IQueryView_Play_PlayStats GetQueryView_Play_PlayStats(this IServiceProvider self) => self.GetService<IQueryView_Play_PlayStats>() ?? throw new InvalidOperationException();
    public static IQueryView_Play_PlayStatsHistory GetQueryView_Play_PlayStatsHistory(this IServiceProvider self) => self.GetService<IQueryView_Play_PlayStatsHistory>() ?? throw new InvalidOperationException();
    public static IQueryView_Play_PlayStats_UserInfo GetQueryView_Play_PlayStats_UserInfo(this IServiceProvider self) => self.GetService<IQueryView_Play_PlayStats_UserInfo>() ?? throw new InvalidOperationException();
    public static IQueryView_UserInfo_UserStats GetQueryView_UserInfo_UserStats(this IServiceProvider self) => self.GetService<IQueryView_UserInfo_UserStats>() ?? throw new InvalidOperationException();
    public static IQueryWebLoginMathResult GetQueryWebLoginMathResult(this IServiceProvider self) => self.GetService<IQueryWebLoginMathResult>() ?? throw new InvalidOperationException();
    public static IQueryUserClassifications GetQueryUserClassifications(this IServiceProvider self) => self.GetService<IQueryUserClassifications>() ?? throw new InvalidOperationException();
    public static IQueryUserSetting GetQueryUserSetting(this IServiceProvider self) => self.GetService<IQueryUserSetting>() ?? throw new InvalidOperationException();
    public static IQueryView_UserAvatarNoBytes GetQueryView_UserAvatarNoBytes(this IServiceProvider self) => self.GetService<IQueryView_UserAvatarNoBytes>() ?? throw new InvalidOperationException();
    public static IQueryLog GetQueryLog(this IServiceProvider self) => self.GetService<IQueryLog>() ?? throw new InvalidOperationException();
    public static IQueryView_UserInfo_UserClassifications GetQueryView_UserInfo_UserClassifications(this IServiceProvider self) => self.GetService<IQueryView_UserInfo_UserClassifications>() ?? throw new InvalidOperationException();
    public static IQueryTokenWithGroup GetQueryTokenWithGroup(this IServiceProvider self) => self.GetService<IQueryTokenWithGroup>() ?? throw new InvalidOperationException();
}