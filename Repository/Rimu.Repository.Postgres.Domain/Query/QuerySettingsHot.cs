using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QuerySettingsHot: IQuerySettingsHot {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QuerySettingsHot(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<Option<SettingsHot>>> GetByMainKeySubKeyAsync(string mainKey, string subKey) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<SettingsHot>(
            $"""
             SELECT *
             FROM "SettingHot"
             WHERE "MainKey" = @MainKey
               AND "SubKey" = @SubKey
             LIMIT 1;
             """,
            new { MainKey = mainKey, SubKey = subKey }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public Task<ResultOk<Option<SettingsHot>>> GetChangeLogs_PathAsync() {
        return GetByMainKeySubKeyAsync("ChangeLogs", "Path");
    }
    
    public Task<ResultOk<Option<SettingsHot>>> GetChangeLogs_UpdateUrlAsync() {
        return GetByMainKeySubKeyAsync("ChangeLogs", "UpdateUrl");
    }
    
    public Task<ResultOk<Option<SettingsHot>>> GetChangeLogs_VersionAsync() {
        return GetByMainKeySubKeyAsync("ChangeLogs", "Version");
    }
    
    public async Task<ResultOk<List<SettingsHot>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryAsync<SettingsHot>(
            $"""
             SELECT * FROM "SettingHot";
             """
        )).Map(x => x.ToList())
          .LogIfError(Logger)
          .ToResultOk();
    }
}