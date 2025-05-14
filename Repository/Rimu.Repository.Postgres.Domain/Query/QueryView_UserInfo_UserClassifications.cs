using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Domain.Query;

public sealed class QueryView_UserInfo_UserClassifications: IQueryView_UserInfo_UserClassifications {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IDbContext _dbContext;

    public QueryView_UserInfo_UserClassifications(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<View_UserInfo_UserClassifications[]>> 
        GetAllWithSingleActiveClassificationsAndPublicShowAsync() {
        
        var db = await _dbContext.GetConnectionAsync();

        const string sql = """
                           SELECT v.*
                           FROM "View_UserInfo_UserClassifications" v 
                           JOIN "UserSetting" us ON v."UserId" = us."UserId"
                           WHERE us."ShowUserClassifications" = true
                             AND (
                                v."CoreDeveloper" = true or
                                v."Developer" = true or
                                v."Contributor" = true or
                                v."Supporter" = true
                             )
                           """;
        return (await db.SafeQueryAsync<View_UserInfo_UserClassifications>(sql))
            .LogIfError(Logger)
            .Map(x => x.ToArray());
    }
}