using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryPlayStatsHistory: IQueryPlayStatsHistory {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryPlayStatsHistory(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<List<PlayStatsHistory>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT * 
                           FROM "PlayStatsHistory"
                           """;
        return (await db.SafeQueryAsync<PlayStatsHistory>(sql))
               .LogIfError(Logger)
               .Map(x => x.ToList());
    }
    
    public async Task<ResultOk<long>> InsertWithNewIdAsync(PlayStatsHistory playStatsHistory) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstAsync<long>(
            $"""
             INSERT INTO "PlayStatsHistory"
                 ("Id", "PlayId", "Score", "Date", "Pp")
             VALUES
                 (nextval('play_score_history_id'), @PlayId, @Score, @Date, @Pp)
             RETURNING "Id"
             """, playStatsHistory
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultNone> InsertBulkWithNewIdAsync(PlayStatsHistory[] playStatsHistories) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync(
            $"""
             INSERT INTO "PlayStatsHistory"
                 ("Id", "PlayId", "Score", "Date", "Pp")
             VALUES
                 (nextval('play_score_history_id'), @PlayId, @Score, @Date, @Pp)
             """, playStatsHistories
        )).LogIfError(Logger).ToNone();
    }
}