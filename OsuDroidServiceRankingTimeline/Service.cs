using OsuDroidLib.Database.Entities;

namespace OsuDroidServiceRankingTimeline;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Response<ServiceState> RunRankingTimeline(ServiceState state) {
        WriteLine("Start Calc New Ranking Timeline");
        var db = DbBuilder.BuildPostSqlAndOpen();
        if (db is null)
            throw new NullReferenceException(nameof(db));
        WriteLine("Finish Calc New Ranking Timeline");
        return Run(db) == EResponse.Ok ? Response<ServiceState>.Ok(state) : Response<ServiceState>.Err;
    }

    private static List<DateTime> GetCalcDays(SavePoco db) {
        var lastTimeV = db.FirstOrDefault<BblGlobalRankingTimeline>(
            "SELECT * FROM bbl_global_ranking_timeline ORDER BY date DESC LIMIT 1").OkOrDefault();
        var firstScore = db.FirstOrDefault<BblScore>(
            "SELECT * FROM bbl_score ORDER BY date ASC LIMIT 1").OkOrDefault();

        if (lastTimeV is null && firstScore is null) return new List<DateTime>(0);

        DateTime startDate;

        if (lastTimeV is null) {
            var scoreDate = firstScore!.Date;
            startDate = new DateTime(scoreDate.Year, scoreDate.Month, scoreDate.Day);
        }

        else {
            var lastCalc = lastTimeV!.Date;
            startDate = new DateTime(lastCalc.Year, lastCalc.Month, lastCalc.Day + 1);
        }

        var res = new List<DateTime>(4);
        var now = DateTime.UtcNow;
        var endDate = new DateTime(now.Year, now.Month, now.Day);

        if (startDate >= endDate) return new List<DateTime>(0);

        var iterDate = startDate;
        while (iterDate < endDate) {
            res.Add(iterDate);
            iterDate = DateTime.SpecifyKind(iterDate.AddDays(1), DateTimeKind.Utc);
        }

        return res;
    }

    private static Response Run(SavePoco db) {
        foreach (var dateTime in GetCalcDays(db)) {
            WriteLine("( Start ) ADD New GlobalTimeLine For: " + Time.ToScyllaString(dateTime));
            var response = CalcTableGlobalForThisDay(dateTime);
            WriteLine("( END   ) ADD New GlobalTimeLine For: " + Time.ToScyllaString(dateTime));
            WriteLine($"Response Ok: {response == EResponse.Ok}");
            if (response == EResponse.Err) return response;
        }

        return Response.Ok();
    }

    private static Response CalcTableGlobalForThisDay(DateTime dateTime) {
        try {
            var date = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            using var db = DbBuilder.BuildNpgsqlConnection();

            var com = db.CreateCommand();

            com.CommandText = @$"
INSERT INTO bbl_global_ranking_timeline (user_id, date, global_ranking, score)
SELECT
    uid as user_id,
    '{Time.ToScyllaString(dateTime)}' As date,
    rank() OVER (ORDER BY score_builder.score DESC, uid DESC) as global_ranking,
    score_builder.score as score
FROM (
         SELECT sum(scores.score) as score, uid
         FROM (
                  SELECT max(score) as score, uid
                  FROM bbl_score
                  WHERE date <= '{Time.ToScyllaString(dateTime)}'
                  GROUP BY hash, uid
              ) as scores
         GROUP BY uid
     ) score_builder
ORDER BY global_ranking;
";
            com.ExecuteNonQuery();
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception e) {
            return Response.Err();
        }
#endif

        return Response.Ok();
    }
}