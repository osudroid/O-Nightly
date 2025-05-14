using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class Query: IQuery {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IDbContext _dbContext;
    
    public Query(IDbContext dbContext) => _dbContext = dbContext;

    public async Task<ResultOk<List<BeatmapTopDto>>> GetBeatmapTop(string filename, string fileHash) {
        var sql = "SELECT beatmap_top(@File, @FileHash, @LimitCount)";
        
        return (await (await _dbContext.GetConnectionAsync()).SafeQueryAsync<BeatmapTopDto>(sql, new {
                    File = filename,
                    FileHash = fileHash,
                    LimitCount = 50
                }
            )).Map(x => {
                      return x.Select(x => {
                              x.Username ??= string.Empty;
                              x.Filename ??= string.Empty;
                              x.FileHash ??= string.Empty;
                              return x;
                          }
                      );
                  }
              )
              .Map(x => x.ToList())
              .LogIfError(Logger)
              .ToResultOk();
    }

    public async Task<ResultOk<List<View_Play_PlayStats_UserInfo>>> PlayRecentFilterByAsync(string filterPlays,
        string orderBy,
        int limit,
        int startAt) {
        static string OrderBy(string orderBy) {
            return orderBy switch {
                "Time_ASC" => "date ASC",
                "Time_DESC" => "date DESC",
                "Score_ASC" => "score ASC",
                "Score_DESC" => "score DESC",
                "Combo_ASC" => "combo ASC",
                "Combo_DESC" => "combo DESC",
                "50_ASC" => "bad ASC",
                "50_DESC" => "bad DESC",
                "100_ASC" => "good ASC",
                "100_DESC" => "good DESC",
                "300_ASC" => "perfect ASC",
                "300_DESC" => "perfect DESC",
                "Miss_ASC" => "miss ASC",
                "Miss_DESC" => "miss DESC",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static string WhereSql(string filterPlays) {
            return filterPlays switch {
                "Any" => string.Empty,
                "XSS_Plays" => "WHERE \"Mark\" = 'XSS'",
                "SS_Plays" => "WHERE \"Mark\" = 'SS'",
                "XS_Plays" => "WHERE \"Mark\" = 'XS'",
                "S_Plays" => "WHERE \"Mark\" = 'S'",
                "A_Plays" => "WHERE \"Mark\" = 'A'",
                "B_Plays" => "WHERE \"Mark\" = 'B'",
                "C_Plays" => "WHERE \"Mark\" = 'C'",
                "D_Plays" => "WHERE \"Mark\" = 'D'",
                "Accuracy_100" => "WHERE \"Accuracy\" = 100000",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        var sql = $"""
SELECT * 
From "View_Play_PlayStats_UserInfo"
{WhereSql(filterPlays)}
ORDER BY {OrderBy(orderBy)} 
LIMIT {limit}
OFFSET {startAt}
""";

        return (await (await _dbContext.GetConnectionAsync()).SafeQueryAsync<View_Play_PlayStats_UserInfo>(sql))
            .Map(x => x.AsList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserInfo.UserRank>>> GetUserRankAsync(long userId, long userOverallScore) {
        var sql = @$"
SELECT t.GlobalRank as globalRank, t.CountryRank as CountryRank
FROM (
         SELECT UserStats.UserId,
                rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as GlobalRank,
                rank() OVER (PARTITION BY bu.Region ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as CountryRank
         FROM UserStats 
                  FULL JOIN UserInfo bu on bu.UserId = UserStats.UserId
         WHERE region IS NOT NULL
         AND OverallScore >= {userOverallScore} AND banned = false
     ) as t
WHERE UserId = {userId}
";
        return (await (await _dbContext.GetConnectionAsync()).SafeQueryFirstOrDefaultAsync<UserInfo.UserRank>(sql))
               .LogIfError(Logger)
               .ToResultOk();
    }


    public async Task<ResultOk<List<LeaderBoardUser>>> LeaderBoardUsersCountry(int limit, string countryNameShortToUpper) {
        var sql = @$"
SELECT rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber, 
       bu.UserId as UserId,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       OverallAccuracy
FROM UserStats
         FULL JOIN UserInfo bu on bu.UserId = UserStats.UserId
WHERE Banned = false AND bu.Region = @Region
ORDER BY RankNumber ASC
LIMIT {limit};
";


        return (await (await _dbContext.GetConnectionAsync()).SafeQueryAsync<LeaderBoardUser>(sql, new { Region = countryNameShortToUpper }))
            .Map(x => x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<List<LeaderBoardUser>>> LeaderBoardUserLikeUserQuery(int limit, string likeUserQuery) {
        var sql = @$"
SELECT RankNumber,
       bu.UserId as UserId,
       username,
       region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       OverallAccuracy
FROM (SELECT rank()
             OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber,
             bu.UserId, Username, bu.Region
      FROM UserStats
               FULL JOIN UserInfo bu on bu.UserId = UserStats.UserId
      WHERE Banned = false
      ORDER BY RankNumber ASC) xx FULL JOIN UserStats bu on bu.UserId = xx.UserId
WHERE lower(xx.Username) LIKE lower(CONCAT(@Username, '%'))
LIMIT {limit}
;";

        return (await (await _dbContext.GetConnectionAsync()).SafeQueryAsync<LeaderBoardUser>(sql, new { Username = likeUserQuery }))
            .Map(x => x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<List<LeaderBoardUser>>> LeaderBoardUserNormal(int limit) {
        var sql = @$"
SELECT rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber, 
       bu.UserId as UserId,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       OverallAccuracy
FROM UserStats
         FULL JOIN UserInfo bu on bu.UserId = UserStats.UserId
WHERE banned = false
ORDER BY RankNumber ASC
LIMIT {limit};
";
        return (await (await _dbContext.GetConnectionAsync()).SafeQueryAsync<LeaderBoardUser>(sql))
            .Map(x => x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<LeaderBoardUser>>> LeaderBoardUserSingleUser(long userId) {
        var sql = $@"
SELECT RankNumber,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       bu.OverallAccuracy
FROM (SELECT rank()
             OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber,
             bu.UserId, username, bu.region
      FROM UserStats
               FULL JOIN UserInfo bu on bu.UserId = UserStats.UserId
      WHERE banned = false
      ORDER BY RankNumber ASC) xx FULL JOIN UserStats bu on bu.UserId = xx.UserId
WHERE xx.UserId = {userId}
;";

        var res = (await (await _dbContext.GetConnectionAsync()).SafeQueryAsync<LeaderBoardUser>(sql))
            .Map(x => x.ToList());

        res.LogIfError(Logger);
        
        if (res == EResult.Err) {
            return ResultOk<Option<LeaderBoardUser>>.Err();
        }
        
        var leaderBord = res.Ok();
        if (leaderBord.Count == 0) {
            return SResult<Option<LeaderBoardUser>>.Ok(Option<LeaderBoardUser>.Empty);
        }
        var single = leaderBord[0];

        return SResult<Option<LeaderBoardUser>>
               .Ok(Option<LeaderBoardUser>.With(single))
               .ToResultOk();
    }
}