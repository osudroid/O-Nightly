using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using NPoco;
using OsuDroid.Utils;

namespace OsuDroid.Database.TableFn;

public static class LeaderBoardUser {
    private static (List<Entities.LeaderBoardUser> List, DateTime createDate) LeaderBoardUserNormalBuffer =
        (new List<Entities.LeaderBoardUser>(0), new DateTime());

    private static readonly ConcurrentDictionary<long, (DateTime dateTime, Entities.LeaderBoardUser leaderBoardUser)>
        LeaderBoardUserSingleUserBuffer = new();

    public static Result<IReadOnlyList<Entities.LeaderBoardUser>, string> LeaderBoardUsersCountry(
        SavePoco db, int limit, CountryInfo.Country country) {
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

        var res = Query(db, sql);
        return res;
    }

    public static Result<IReadOnlyList<Entities.LeaderBoardUser>, string> LeaderBoardUserLikeUserQuery(
        SavePoco db, int limit, string likeUserQuery) {
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
", likeUserQuery.ToLower() + "%");

        return Query(db, sql);
    }

    public static Result<IReadOnlyList<Entities.LeaderBoardUser>, string> LeaderBoardUserNormal(SavePoco db, int limit) {
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
        if (LeaderBoardUserNormalBuffer.List.Count >= limit
            && LeaderBoardUserNormalBuffer.createDate + TimeSpan.FromMinutes(5) > DateTime.UtcNow)
            return Result<IReadOnlyList<Entities.LeaderBoardUser>, string>
                .Ok(CollectionsMarshal.AsSpan(LeaderBoardUserNormalBuffer.List).Slice(0, limit).ToArray());

        var res = Query(db, sql);
        if (res == EResult.Err)
            return Result<IReadOnlyList<Entities.LeaderBoardUser>, string>.Err(res.Err());
        
        LeaderBoardUserNormalBuffer = ((List<Entities.LeaderBoardUser>)res.Ok(), DateTime.UtcNow);
        return res;
    }

    public static Result<Option<Entities.LeaderBoardUser>, string> LeaderBoardUserSingleUser(SavePoco db, long userId) {
        if (LeaderBoardUserSingleUserBuffer.TryGetValue(userId, out var now))
            if (now.dateTime > DateTime.UtcNow - TimeSpan.FromSeconds(10))
                return Result<Option<Entities.LeaderBoardUser>, string>
                    .Ok(Option<Entities.LeaderBoardUser>.With(now.leaderBoardUser));

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

        var res = Query(db, sql);
        if (res == EResult.Err)
            return Result<Option<Entities.LeaderBoardUser>, string>.Err(res.Err());
        var leaderBord = res.Ok();
        if (leaderBord.Count == 0)
            return Result<Option<Entities.LeaderBoardUser>, string>.Ok(Option<Entities.LeaderBoardUser>.Empty);
        var single = leaderBord[0];

        LeaderBoardUserSingleUserBuffer[single.Id] = (DateTime.UtcNow, single);

        return Result<Option<Entities.LeaderBoardUser>, string>.Ok(Option<Entities.LeaderBoardUser>.With(single));
    }

    private static Result<IReadOnlyList<Entities.LeaderBoardUser>, string> Query(SavePoco db, Sql sql) 
        => db.Fetch<Entities.LeaderBoardUser>(sql).Map(x => (IReadOnlyList<Entities.LeaderBoardUser>)x);
}