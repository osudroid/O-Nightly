using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Postgres.Domain;
using Rimu.Repository.Postgres.Domain.Query;

namespace Rimu.Repository.Postgres.Binder;

public class ServicePostgesBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<IDbContext>(x => new DbContext());
        
        serviceCollection.AddScoped<IQuery>(x => new Query(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryGlobalRankingTimeline>(x => new QueryGlobalRankingTimeline(x.GetDbContext(), x.GetQuery()));
        serviceCollection.AddScoped<IQueryPlay>(x => new QueryPlay(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryPlayStats>(x => new QueryPlayStats(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryPlayStatsHistory>(x => new QueryPlayStatsHistory(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryReplayFile>(x => new QueryReplayFile(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryResetPasswordKey>(x => new QueryResetPasswordKey(x.GetDbContext()));
        serviceCollection.AddScoped<IQuerySetting>(x => new QuerySetting(x.GetDbContext()));
        serviceCollection.AddScoped<IQuerySettingsHot>(x => new QuerySettingsHot(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryTokenUser>(x => new QueryTokenUser(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryUserAvatar>(x => new QueryUserAvatar(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryUserInfo>(x => new QueryUserInfo(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryUserStats>(x => new QueryUserStats(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryView_Play_PlayStats>(x => new QueryView_Play_PlayStats(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryView_Play_PlayStatsHistory>(x => new QueryView_Play_PlayStatsHistory(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryView_Play_PlayStats_UserInfo>(x => new QueryView_Play_PlayStats_UserInfo(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryView_UserInfo_UserStats>(x => new QueryView_UserInfo_UserStats(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryWebLoginMathResult>(x => new QueryWebLoginMathResult(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryLog>(x => new QueryLog(x.GetDbContext()));
        serviceCollection.AddScoped<IDbTransactionContext>(x => new DbTransactionContext(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryUserClassifications>(x => new QueryUserClassifications(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryUserSetting>(x => new QueryUserSetting(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryView_UserAvatarNoBytes>(x => new QueryView_UserAvatarNoBytes(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryView_UserInfo_UserClassifications>(x => new QueryView_UserInfo_UserClassifications(x.GetDbContext()));
        serviceCollection.AddScoped<IQueryTokenWithGroup>(x => new QueryTokenWithGroup(x.GetDbContext()));
    }
}