using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using UserClassificationsDto = Rimu.Repository.Postgres.Adapter.Dto.UserClassificationsDto;

namespace Rimu.Repository.Postgres.Domain.Query;

public sealed class QueryUserClassifications: IQueryUserClassifications {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IDbContext _dbContext;

    public QueryUserClassifications(IDbContext dbContext) => _dbContext = dbContext;


    public async Task<ResultNone> InsertAsync(UserClassifications iUserClassificationsReadonly) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           INSERT INTO "UserClassifications" 
                               ("UserId", "CoreDeveloper", "Developer", "Contributor", "Supporter") 
                           VALUES 
                               (@UserId, @CoreDeveloper, @Developer, @Contributor, @Supporter)
                           """;
        return (await db.SafeExecuteAsync(sql, iUserClassificationsReadonly))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> InsertBulkAsync(UserClassifications[] userClassificationsArray) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           INSERT INTO "UserClassifications" 
                               ("UserId", "CoreDeveloper", "Developer", "Contributor", "Supporter") 
                           VALUES 
                               (@UserId, @CoreDeveloper, @Developer, @Contributor, @Supporter)
                           """;
        return (await db.SafeExecuteAsync(sql, userClassificationsArray))
               .LogIfError(Logger)
               .ToNone();
    }

    public async Task<ResultOk<List<UserClassifications>>> GetAllCoreDeveloperOrDeveloperOrContributorOrSupporterAsync() {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT * 
                           FROM "UserClassifications" 
                           WHERE "CoreDeveloper" = true 
                              OR "Developer" = true
                              OR "Contributor" = true
                              OR "Supporter" = true
                           ;
                           """;
        return (await db.SafeQueryAsync<UserClassifications>(sql))
               .LogIfError(Logger)
               .Map(x => x.ToList())
               .ToResultOk();
    }

    public async Task<ResultNone> UpdateAsync(UserClassifications iUserClassificationsReadonly) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           Update "UserClassifications" 
                           SET "UserId" = @UserId,
                               "CoreDeveloper" = @CoreDeveloper, 
                               "Developer" = @Developer, 
                               "Contributor" = @Contributor, 
                               "Supporter" = @Supporter 
                           Where "UserId" = @UserId
                           """;
        return (await db.SafeExecuteAsync(sql, iUserClassificationsReadonly))
               .LogIfError(Logger)
               .ToNone();
    }

    public async Task<ResultNone> DeleteAsync(long id) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           DELETE FROM "UserClassifications"
                           WHERE "UserId" = @UserId
                           """;
        return (await db.SafeExecuteAsync(sql, new { UserId = id }))
               .LogIfError(Logger)
               .ToNone();
    }

    public async Task<ResultOk<Option<UserClassifications>>> GetByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                   SELECT * 
                   FROM "UserClassifications"
                   WHERE "UserId" = @UserId
                   LIMIT 1
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserClassifications>(sql, new { UserId = userId }))
               .LogIfError(Logger)
               .ToResultOk();
    }
}