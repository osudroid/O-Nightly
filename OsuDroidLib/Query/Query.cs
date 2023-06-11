using Dapper;
using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;

namespace OsuDroidLib.Query; 

public static class Query {
    public static async Task<Result<IEnumerable<PlayScoreWithUsername>,string>> PlayRecentFilterByAsync(
        NpgsqlConnection db,
        string filterPlays,
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
                "XSS_Plays" => "WHERE mark = 'XSS'",
                "SS_Plays" => "WHERE mark = 'SS'",
                "XS_Plays" => "WHERE mark = 'XS'",
                "S_Plays" => "WHERE mark = 'S'",
                "A_Plays" => "WHERE mark = 'A'",
                "B_Plays" => "WHERE mark = 'B'",
                "C_Plays" => "WHERE mark = 'C'",
                "D_Plays" => "WHERE mark = 'D'",
                "Accuracy_100" => "WHERE accuracy = 100000",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        var sql = $@"
SELECT PlayScore.*, bus.username as Username 
From PlayScore
JOIN UserInfo bus on PlayScore.UserId = bus.UserId
{WhereSql(filterPlays)}
ORDER BY {OrderBy(orderBy)} 
LIMIT {limit}
OFFSET {startAt}
";

        return await db.SafeQueryAsync<PlayScoreWithUsername>(sql);
    }

    public static async Task<Result<Option<UserInfoAndBblUserStats>, string>> GetUserInfoAndBblUserStatsByUserIdAsync(
        NpgsqlConnection db, long userId) {
        
        var sql = $@"
SELECT 
    bus.UserId as UserId,
    Username,
    Password,
    Email,
    DeviceId,
    RegisterTime,
    LastLoginTime,
    Region,
    Active,
    Banned,
    RestrictMode,
    UsernameLastChange,
    LatestIp,
    PatronEmail,
    PatronEmailAccept,
    OverallPlaycount,
    OverallScore,
    OverallAccuracy,
    OverallCombo,
    OverallXss,
    OverallSs,
    OverallXs,
    OverallS,
    OverallA,
    OverallB,
    OverallC,
    OverallD,
    OverallHits,
    Overall300,
    Overall100,
    Overall50,
    OverallGeki,
    OverallKatu,
    OverallMiss,
FROM UserInfo 
    JOIN UserStats bus on bus.UserId = UserInfo.UserId 
WHERE UserInfo.UserId = {userId}
";
        return await db.SafeQueryFirstOrDefaultAsync<UserInfoAndBblUserStats>(sql);
    }

    public static async Task<Result<Option<UserInfo.UserRank>, string>> GetUserRankAsync(
        NpgsqlConnection db, long userId, long userOverallScore) {

        var sql = @$"
SELECT t.GlobalRank as globalRank, t.CountryRank as CountryRank
FROM (
         SELECT UserId,
                rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as GlobalRank,
                rank() OVER (PARTITION BY bu.Region ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as CountryRank
         FROM UserStats
                  FULL JOIN UserInfo bu on bu.UserId = UserStats.UserId
         WHERE region IS NOT NULL
         AND OverallScore >= {userOverallScore} AND banned = false
     ) as t
WHERE UserId = {userId}
";
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo.UserRank>(sql);
    }
    
    
        public static async Task<Result<List<LeaderBoardUser>, string>> LeaderBoardUsersCountry(
        NpgsqlConnection db, int limit, CountryInfo.Country country) {

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


        return (await db.SafeQueryAsync<LeaderBoardUser>(sql, new { Region = country.NameShort.ToUpper() }))
            .Map(x => x.ToList());
    }

    public static async Task<Result<List<LeaderBoardUser>, string>> LeaderBoardUserLikeUserQuery(
        NpgsqlConnection db, int limit, string likeUserQuery) {
        
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
        
        return (await db.SafeQueryAsync<LeaderBoardUser>(sql, new { Username = likeUserQuery }))
            .Map(x => x.ToList());
    }

    public static async Task<Result<List<LeaderBoardUser>, string>> LeaderBoardUserNormal(NpgsqlConnection db, int limit) {
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
        return (await db.SafeQueryAsync<LeaderBoardUser>(sql))
            .Map(x => x.ToList());
    }

    public static async Task<Result<Option<LeaderBoardUser>, string>> LeaderBoardUserSingleUser(NpgsqlConnection db, long userId) {
        
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

        var res = (await db.SafeQueryAsync<LeaderBoardUser>(sql))
            .Map(x => x.ToList());
        
        if (res == EResult.Err)
            return Result<Option<LeaderBoardUser>, string>.Err(res.Err());
        var leaderBord = res.Ok();
        if (leaderBord.Count == 0)
            return Result<Option<LeaderBoardUser>, string>.Ok(Option<LeaderBoardUser>.Empty);
        var single = leaderBord[0];

        return Result<Option<LeaderBoardUser>, string>.Ok(Option<LeaderBoardUser>.With(single));
    }
}