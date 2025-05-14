using System.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Terminal.Provider;

internal class CleanDbProvider {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IDbContext _dbContext;

    public CleanDbProvider(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    /// <exception></exception>
    public async Task RunForTransferAsync() {
        var db = await _dbContext.GetConnectionAsync();

        Logger.Info("Start TRUNCATE DB");
        await db.ExecuteAsync(
            """
            TRUNCATE public."GlobalRankingTimeline";
            TRUNCATE public."OldMarkNewMark";
            TRUNCATE public."PlayStats" CASCADE;
            TRUNCATE public."PlayStatsHistory";
            TRUNCATE public."Play" CASCADE;
            TRUNCATE public."ResetPasswordKey";
            TRUNCATE public."TokenUser";
            TRUNCATE public."UserAvatar";
            TRUNCATE public."UserStats";
            TRUNCATE public."WebLoginMathResult";
            TRUNCATE public."UserInfo" CASCADE;
            """
            );
        Logger.Info("END TRUNCATE DB");
    }

    public async Task RunForRankingTimelineAsync() {
        var db = await _dbContext.GetConnectionAsync();

        Logger.Info("Start TRUNCATE DB");
        await db.ExecuteAsync(
            """
            TRUNCATE public."GlobalRankingTimeline";
            """
        );
        Logger.Info("END TRUNCATE DB");
    }
}