using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryPlay: IQueryPlay {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryPlay(IDbContext dbContext) => _dbContext = dbContext;

    public async Task<ResultOk<long>> InsertIfNotExistAsync(Play play) {
        var db = await _dbContext.GetConnectionAsync();
        var res = (await db.SafeQueryFirstOrDefaultAsync<Play>(
            $"""
              SELECT * 
              FROM "Play"
              WHERE "UserId" = @UserId
                AND "Filename" = @Filename
                AND "FileHash" = @FileHash
             """, play
        ))
            .LogIfError(Logger)
            .ToResultOk();
        
        if (res == EResult.Err) {
            return ResultOk<long>.Err();
        }

        if (res.Ok().IsSet()) {
            return res.Map(x => x.Unwrap().Id);
        }
        
        return (await db.SafeQueryFirstAsync<long>(
            $"""
              INSERT INTO "Play"
                  ("UserId", "Filename", "FileHash") 
              VALUES 
                  (@UserId, @Filename, @FileHash)
              returning "Id"
             """, play
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<Option<Play>>> GetByIdAsync(long id) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<Play>(
            $"""SELECT * FROM "Play" WHERE "Id" = @Id""", new
                { Id = id }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<Option<Play>>> GetByUserIdFilenameFileHashAsync(
        long id, 
        string filename, 
        string fileHash) {
        
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<Play>(
            $"""
             SELECT * FROM "Play" 
                      WHERE "Id" = @Id 
                        AND "Filename" = @Filename
                        AND "FileHash" = @FileHash
             """,
            new { Id = id, Filename = filename, FileHash = fileHash }
        )).LogIfError(Logger).ToResultOk();
    }

    public async Task<ResultNone> InsertBulkAsync(Play[] plays) {
        var db = await _dbContext.GetConnectionAsync();
        var query = 
            """
        INSERT INTO "Play" 
                (
                    "Id",
                    "UserId",
                    "Filename",
                    "FileHash" 
                )
                VALUES 
                    (
                        @Id,
                        @UserId,
                        @Filename,
                        @FileHash
                    )
        """;
        
        return (await db.SafeExecuteAsync(query, plays))
               .LogIfError(Logger)
               .ToNone();
    }
}