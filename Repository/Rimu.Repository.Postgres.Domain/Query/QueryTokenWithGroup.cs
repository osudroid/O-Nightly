using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public sealed class QueryTokenWithGroup: IQueryTokenWithGroup {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryTokenWithGroup(IDbContext dbContext) => _dbContext = dbContext;

    public async Task<ResultNone> InsertAsync(TokenWithGroup value) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           INSERT INTO "TokenWithGroup"
                              ("Group", "Token", "CreateTime", "Data") 
                           VALUES 
                              (@Group, @Token, @CreateTime, @Data)
                           """;

        return (await db.SafeQueryAsync(sql, value))
            .LogIfError(Logger)
            .ToNone()
        ;
    }

    public async Task<ResultNone> DeleteAsync(ETokenGroup tokenGroup, string token) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           DELETE FROM "TokenWithGroup" 
                           WHERE "Group" = @Group,
                             AND "Token" = @Token
                           """;
        
        return (await db.SafeQueryAsync(sql, new { Group = tokenGroup.ToString(), Token = token }))
               .LogIfError(Logger)
               .ToNone()
            ;
    }

    public async Task<ResultOk<Option<TokenWithGroup>>> FindByTokenGroupAndTokenAsync(
        ETokenGroup tokenGroup, 
        string token) {
        
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT * FROM "TokenWithGroup" 
                           WHERE "Group" = @Group,
                             AND "Token" = @Token
                           """;

        return (await db.SafeQueryFirstOrDefaultAsync<TokenWithGroup>(sql, new { Group = tokenGroup.ToString(), Token = token }))
            .LogIfError(Logger)
            ;
    }
}