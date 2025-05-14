using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryLog: IQueryLog {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IDbContext _dbContext;

    public QueryLog(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultNone> InsertAsync(Log log) {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = 
            """
            INSERT INTO "Log" (
                "Id",
                "Date",
                "DateTime",
                "Message",
                "Status",
                "Stack",
                "Trigger"
            ) VALUES (
                @Id,
                @Date,
                @DateTime,
                @Message,
                @Status,
                @Stack,
                @Trigger
            )
            """;

        return (await db.SafeExecuteAsync(sql, log))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> InsertBulkAsync(Log[] logs) {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = 
            """
            INSERT INTO "Log" (
                "Id",
                "Date",
                "DateTime",
                "Message",
                "Status",
                "Stack",
                "Trigger"
            ) VALUES (
                @Id,
                @Date,
                @DateTime,
                @Message,
                @Status,
                @Stack,
                @Trigger
            )
            """;

        return (await db.SafeExecuteAsync(sql, logs))
               .LogIfError(Logger)
               .ToNone();
    }
}