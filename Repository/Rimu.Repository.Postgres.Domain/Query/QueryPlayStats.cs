using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryPlayStats: IQueryPlayStats {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryPlayStats(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<List<PlayStats>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryAsync<PlayStats>("SELECT * FROM \"PlayStats\""))
            .Map(x=>x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }
    public async Task<ResultNone> InsertAsync(PlayStats playStats) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync(
            """
            INSERT INTO "PlayStats"
            (
                "Id",
                "ReplayFileId",
                "Mode",
                "Score",
                "Combo",
                "Mark",
                "Geki",
                "Perfect",
                "Katu",
                "Good",
                "Bad",
                "Miss",
                "Date",
                "Accuracy",
                "Pp"
            ) VALUES (
                @Id,
                @ReplayFileId,
                @Mode,
                @Score,
                @Combo,
                @Mark,
                @Geki,
                @Perfect,
                @Katu,
                @Good,
                @Bad,
                @Miss,
                @Date,
                @Accuracy,
                @Pp
            )
                                                     
            """, playStats
        )).LogIfError(Logger)
          .ToNone();
    }
    
    public async Task<ResultNone> UpdateAsync(PlayStats playStats) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync(
            """
            Update "PlayStats" 
            SET
                 "ReplayFileId" = @ReplayFileId,
                 "Mode" = @Mode,
                 "Score" = @Score,
                 "Combo" = @Combo,
                 "Mark" = @Mark,
                 "Geki" = @Geki,
                 "Perfect" = @Perfect,
                 "Katu" = @Katu,
                 "Good" = @Good,
                 "Bad" = @Bad,
                 "Miss" = @Miss,
                 "Date" = @Date,
                 "Accuracy" = @Accuracy,
                 "Pp" = @Pp
            WHERE "Id" = @Id                  
            """, playStats
        )).LogIfError(Logger).ToNone();
    }

    public async Task<ResultNone> InsertBulkAsync(PlayStats[] playStatss) {
        var db = await _dbContext.GetConnectionAsync();
        var query = """
                    INSERT INTO "PlayStats"
                    (
                        "Id",
                        "ReplayFileId",
                        "Mode",
                        "Score",
                        "Combo",
                        "Mark",
                        "Geki",
                        "Perfect",
                        "Katu",
                        "Good",
                        "Bad",
                        "Miss",
                        "Date",
                        "Accuracy",
                        "Pp"
                    ) VALUES (
                        @Id,
                        @ReplayFileId,
                        @Mode,
                        @Score,
                        @Combo,
                        @Mark,
                        @Geki,
                        @Perfect,
                        @Katu,
                        @Good,
                        @Bad,
                        @Miss,
                        @Date,
                        @Accuracy,
                        @Pp
                    )
                                                             
                    """;
        return (await db.SafeExecuteAsync(query, playStatss))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> DeleteByIdAsync(long id) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           DELETE FROM "PlayStats"
                           WHERE "Id" = @Id
                           """;

        return (await db.SafeExecuteAsync(sql, new { Id = id }))
               .LogIfError(Logger)
               .ToNone();
    }
}