using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryReplayFile: IQueryReplayFile {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryReplayFile(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<long>> InsertAsync(byte[] odr) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstAsync<long>(
            """
              INSERT INTO "ReplayFile" 
                  ("Odr") 
              VALUES 
                  (@Odr)
              RETURNING "Id"
            """, new { Odr = odr }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultNone> BulkInsertWithIdsAsync(ReplayFile[] replayFiles) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync(
                """
                  INSERT INTO "ReplayFile" 
                      ("Id","Odr") 
                  VALUES 
                      (@Id , @Odr)
                  RETURNING "Id"
                """, replayFiles
            )).LogIfError(Logger)
              .ToNone();
    }

    public async Task<ResultOk<Option<ReplayFile>>> GetByIdAsync(long id) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<ReplayFile>(
            $"""
                 SELECT * FROM "ReplayFile" where "Id" = {id}
             """
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<long[]>> GetAllIdsAsync() {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = """ SELECT "Id" FROM "ReplayFile" """;
        
        return (await db.SafeQueryAsync<ReplayFile>(sql))
               .Map(static s => s.Select(static x => x.Id).ToArray())
               .LogIfError(Logger);
    }
}