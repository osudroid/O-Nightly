using Dapper;
using LamLibAllOver;
using Npgsql;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryView_Play_PlayStatsHistory: IQueryView_Play_PlayStatsHistory {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryView_Play_PlayStatsHistory(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<List<View_Play_PlayStatsHistory>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = "SELECT * FROM \"View_Play_PlayStatsHistory\"";
        var reponse =  (await db.SafeQueryAsync<View_Play_PlayStatsHistory>(sql))
            .LogIfError(Logger)
            .Map(x=>x.ToList());

        return reponse;
    } 
}