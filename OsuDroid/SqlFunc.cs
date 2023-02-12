using NPoco;
using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;

namespace OsuDroid;

public static class SqlFunc {
    public static Result<BblScore, string> GetBblScoreById(SavePoco db, long id) {
        return db.Single<BblScore>(@$"
SELECT * FROM bbl_score WHERE id = {id}
");
    }

    public static Result<BblScore, string> GetBblScoreByIdAndUserId(SavePoco db, long id, long userId) {
        return db.Single<BblScore>(@$"
SELECT * FROM bbl_score WHERE id = {id} AND uid = {userId}
");
    }

    public static Result<BblScore, string> GetBblScoreOldesByUserIdAndHash(SavePoco db, long userId, string mapHash) {
        return db.Single<BblScore>(@$"
SELECT * 
FROM bbl_score 
WHERE uid = {userId}
AND hash = {mapHash}
ORDER BY date DESC 
LIMIT 1
");
    }

    public static ResultErr<string> InsertBblScore(SavePoco db, BblScore newScoreInsert) {
        var sql = new Sql(@$"
INSERT 
INTO bbl_score (id, uid, filename, hash, mode, score, combo, mark, geki, perfect, katu, good, bad, miss, date, accuracy) 
VALUES
    (
     {newScoreInsert.Id},
     {newScoreInsert.Uid},
     @0,
     @1,
     @2,
     {newScoreInsert.Score},
     {newScoreInsert.Combo},
     @3,
     {newScoreInsert.Geki},
     {newScoreInsert.Perfect},
     {newScoreInsert.Katu},
     {newScoreInsert.Good},
     {newScoreInsert.Bad},
     {newScoreInsert.Miss},
     '{Time.ToScyllaString(newScoreInsert.Date)}',
     {newScoreInsert.Accuracy}
    ) 
", newScoreInsert.Filename!, newScoreInsert.Hash!, newScoreInsert.Mode, newScoreInsert.Mark);
        return db.Execute(sql);
    }

    public static Result<BblUserStats, string> GetBblUserStatsByUserId(SavePoco db, long userId) {
        return db.SingleOrDefault<BblUserStats>(@$"
SELECT * 
FROM bbl_user_stats
WHERE uid = {userId}
LIMIT 1
");
    }

    public static Result<List<LeaderBoardUser>, string> LeaderBoardFilterCountry(SavePoco db, int limit,
        CountryInfo.Country country) {
        var sql = new Sql(@$"
SELECT rank() OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as rank_number, 
       bu.id as id,
       username,
       region,
       overall_score,
       playcount,
       overall_ss,
       overall_s,
       overall_a,
       overall_accuracy
FROM bbl_user_stats
         FULL JOIN bbl_user bu on bu.id = bbl_user_stats.uid
WHERE banned = false AND bu.region = @0
ORDER BY rank_number ASC
LIMIT {limit};
", country.NameShort.ToUpper());

        return db.Fetch<LeaderBoardUser>(sql);
    }

    public static Result<Option<LeaderBoardUser>, string> LeaderBoardUserRank(SavePoco db, long userId) {
        var sql = new Sql(@"
SELECT rank_number,
       username,
       region,
       overall_score,
       playcount,
       overall_ss,
       overall_s,
       overall_a,
       bu.overall_accuracy
FROM (SELECT rank()
             OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as rank_number,
             bu.id, username, bu.region
      FROM bbl_user_stats
               FULL JOIN bbl_user bu on bu.id = bbl_user_stats.uid
      WHERE banned = false
      ORDER BY rank_number ASC) xx FULL JOIN bbl_user_stats bu on bu.uid = xx.id
WHERE xx.id = @0
;
", userId);

        return db.Fetch<LeaderBoardUser>(sql)
            .Map(x => Option<LeaderBoardUser>.NullSplit(x.FirstOrDefault()));
    }

    public static Result<List<LeaderBoardUser>, string> LeaderBoardNoFilter(SavePoco db, int limit) {
        var sql = new Sql(@$"
SELECT rank() OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as rank_number, 
       bu.id as id,
       username,
       region,
       overall_score,
       playcount,
       overall_ss,
       overall_s,
       overall_a,
       overall_accuracy
FROM bbl_user_stats
         FULL JOIN bbl_user bu on bu.id = bbl_user_stats.uid
WHERE banned = false
ORDER BY rank_number ASC
LIMIT {limit};
");
        return db.Fetch<LeaderBoardUser>(sql);
    }

    public static Result<List<LeaderBoardUser>, string> LeaderBoardSearchUser(SavePoco db, long limit, string query) {
        var sql = new Sql(@$"
SELECT rank_number,
       bu.uid as id,
       username,
       region,
       overall_score,
       playcount,
       overall_ss,
       overall_s,
       overall_a,
       bu.overall_accuracy
FROM (SELECT rank()
             OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as rank_number,
             bu.id, username, bu.region
      FROM bbl_user_stats
               FULL JOIN bbl_user bu on bu.id = bbl_user_stats.uid
      WHERE banned = false
      ORDER BY rank_number ASC) xx FULL JOIN bbl_user_stats bu on bu.uid = xx.id
WHERE lower(xx.username) LIKE @0
LIMIT {limit}
;
", query.ToLower() + "%");

        return db.Fetch<LeaderBoardUser>(sql);
    }

    public static Result<List<LeaderBoardUser>, string> LeaderBoardSearchUser(SavePoco db, long limit, string query,
        CountryInfo.Country country) {
        var sql = new Sql(@$"
SELECT rank_number,
       bu.uid as id,
       username,
       region,
       overall_score,
       playcount,
       overall_ss,
       overall_s,
       overall_a,
       bu.overall_accuracy
FROM (SELECT rank()
             OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as rank_number,
             bu.id, username, bu.region
      FROM bbl_user_stats
               FULL JOIN bbl_user bu on bu.id = bbl_user_stats.uid
      WHERE banned = false
      ORDER BY rank_number ASC) xx FULL JOIN bbl_user_stats bu on bu.uid = xx.id
WHERE lower(xx.username) LIKE @0
AND region = @1
LIMIT {limit}
;
", query.ToLower() + "%", country.NameShort);
        return db.Fetch<LeaderBoardUser>(sql);
    }

    public static Result<StatisticActiveUser, string> GetStatisticActiveUser(SavePoco db) {
        return db.SingleOrDefault<StatisticActiveUser>(@"
SELECT 
    count(*) as register_user,
    sum(CASE WHEN last_login_time >= @0 THEN 1 ELSE 0 END) as active_user_last_1h,
    sum(CASE WHEN last_login_time >= @1 THEN 1 ELSE 0 END) as active_user_last_1day
FROM bbl_user 
WHERE banned = false 
", DateTime.UtcNow - TimeSpan.FromHours(1), DateTime.UtcNow - TimeSpan.FromDays(1));
    }

    public static Result<List<(string Username, long Id)>, string> GetPatronUser(SavePoco db) {
        var sql = new Sql(@"
SELECT bu.username, bu.id
FROM bbl_patron 
JOIN bbl_user bu on bbl_patron.patron_email = bu.patron_email
WHERE active_supporter = true 
  and bu.active = true 
");
        
        return db.Fetch<BblUser>(sql).Map(list => {
            var res = new List<(string Username, long Id)>(list.Count);

            foreach (var bblPatron in list)
                res.Add((
                    bblPatron.Username!,
                    bblPatron.Id!
                ));

            return res;
        });
    }

    public sealed class StatisticActiveUser {
        [Column("active_user_last_1h")] public long ActiveUserLast1h { get; set; }
        [Column("active_user_last_1day")] public long ActiveUserLast1Day { get; set; }
        [Column("register_user")] public long RegisterUser { get; set; }
    }
}