using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QuerySetting: IQuerySetting {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QuerySetting(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<Option<Setting>>> GetSetting(string mainKey, string subKey) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT * 
FROM "Setting"
WHERE "MainKey" = @MainKey
AND "SubKey" = @SubKey
""";
        return (await db.SafeQueryFirstOrDefaultAsync<Setting>(
            sql,
            new { MainKey = mainKey, SubKey = subKey }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<Setting>>> GetSettingsAsync(string mainKey) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT * 
FROM "Setting"
WHERE "MainKey" = @MainKey
""";

        return (await db.SafeQueryAsync<Setting>(sql, new { MainKey = mainKey }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<Setting>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """ SELECT * FROM "Setting" """;

        return (await db.SafeQueryAsync<Setting>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }
}