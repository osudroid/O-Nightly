using Dapper;
using LamLibAllOver;
using LamLibAllOver.ErrorHandling;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryWebLoginMathResult: IQueryWebLoginMathResult {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryWebLoginMathResult(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultNone> AddWebLoginTokenAsync(WebLoginMathResult webLoginMathResult) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  INSERT INTO "WebLoginMathResult" 
                      ("WebLoginMathResultId", "CreateTime", "MathResult") 
                  VALUES 
                      (@WebLoginMathResultId, @CreateTime, @MathResult)
                  """;
        
        return (await db.SafeExecuteAsync(sql, new {
                webLoginMathResult.WebLoginMathResultId,
                webLoginMathResult.CreateTime,
                webLoginMathResult.MathResult
            }
        )).LogIfError(Logger)
          .ToNone();
    }

    public async Task<ResultNone> DeleteWebLoginTokenAsync(Guid id) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  DELETE 
                  FROM "WebLoginMathResult" 
                  WHERE "WebLoginMathResultId" = @WebLoginMathResultId
                  """;
        
        return (await db.SafeExecuteAsync(sql, new { WebLoginMathResultId = id }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultOk<Option<WebLoginMathResult>>> GetWebLoginTokenAsync(Guid id) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  SELECT *
                  FROM "WebLoginMathResult" 
                  WHERE "WebLoginMathResultId" = @WebLoginMathResultId
                  """;
        
        var result = (await db.SafeQueryFirstOrDefaultAsync<WebLoginMathResult>(
            sql, 
            new { WebLoginMathResultId = id }))
            .LogIfError(Logger)
            .ToResultOk();

        return result;
    }

    public async Task<ResultNone> DeleteOldTokens(TimeSpan timeSpan) {
        var db = await _dbContext.GetConnectionAsync();
        var time = DateTime.UtcNow.Add(timeSpan);

        var sql = """
                  DELETE 
                  FROM "WebLoginMathResult"
                  WHERE "CreateTime" <= @Date 
                  """;
        
        var result = (await db.SafeExecuteAsync(sql, new { Date = time }))
            .LogIfError(Logger)
            .ToNone();

        return result;
    }
}