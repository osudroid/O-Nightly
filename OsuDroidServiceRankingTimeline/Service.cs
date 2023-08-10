using Npgsql;
using OsuDroidLib.Extension;

namespace OsuDroidServiceRankingTimeline;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Result<ServiceState, string> RunRankingTimeline(ServiceState state) {
        WriteLine("Start Calc New Ranking Timeline");
        var db = DbBuilder.BuildNpgsqlConnection().GetAwaiter().GetResult();
        if (db is null)
            throw new NullReferenceException(nameof(db));
        WriteLine("Finish Calc New Ranking Timeline");
        var resultErr = Run(db).GetAwaiter().GetResult();

        return resultErr == EResult.Err
            ? Result<ServiceState, string>.Err(resultErr.Err())
            : Result<ServiceState, string>.Ok(state);
    }

    private static async Task<List<DateTime>> GetCalcDays(NpgsqlConnection db) {
        var lastTimeVResult = await db.SafeQueryFirstOrDefaultAsync<Entities.GlobalRankingTimeline>(
            "SELECT * FROM GlobalRankingTimeline ORDER BY date DESC LIMIT 1"
        );
        var firstScoreResult = await db.SafeQueryFirstOrDefaultAsync<Entities.PlayScore>(
            "SELECT * FROM PlayScore ORDER BY date ASC LIMIT 1"
        );

        if (lastTimeVResult == EResult.Err) {
            WriteLine(lastTimeVResult.Err());
            return new List<DateTime>(0);
        }

        if (firstScoreResult == EResult.Err) {
            WriteLine(lastTimeVResult.Err());
            return new List<DateTime>(0);
        }

        var lastTimeVOption = lastTimeVResult.Ok();
        var firstScoreOption = firstScoreResult.Ok();

        if (lastTimeVOption.IsNotSet() && firstScoreOption.IsNotSet()) return new List<DateTime>(0);

        DateTime startDate;

        if (lastTimeVOption.IsNotSet()) {
            var scoreDate = firstScoreOption.Unwrap().Date;
            startDate = new DateTime(scoreDate.Year, scoreDate.Month, scoreDate.Day);
        }

        else {
            var lastCalc = lastTimeVOption.Unwrap().Date;
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

    private static async Task<ResultErr<string>> Run(NpgsqlConnection db) {
        foreach (var dateTime in await GetCalcDays(db)) {
            WriteLine("( Start ) ADD New GlobalTimeLine For: " + Time.ToScyllaString(dateTime));
            var response = await CalcTableGlobalForThisDay(dateTime);
            WriteLine("( END   ) ADD New GlobalTimeLine For: " + Time.ToScyllaString(dateTime));
            WriteLine($"Response Ok: {response == EResult.Err}");
            if (response == EResult.Err) WriteLine("Response Message: " + response.Err());

            return response;
        }

        return ResultErr<string>.Ok();
    }

    private static async Task<ResultErr<string>> CalcTableGlobalForThisDay(DateTime dateTime) {
        var date = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        using var db = DbBuilder.BuildNpgsqlConnection().GetAwaiter().GetResult();

        var com = await db.SafeQueryAsync(@$"
INSERT INTO GlobalRankingTimeline (UserId, Date, GlobalRanking, Score)
SELECT
    UserId as UserId,
    '{Time.ToScyllaString(dateTime)}' As Date,
    rank() OVER (ORDER BY ScoreBuilder.Score DESC, UserId DESC) as global_ranking,
    ScoreBuilder.score as score
FROM (
         SELECT sum(scores.score) as Score, UserId
         FROM (
                  SELECT max(score) as Score, UserId
                  FROM PlayScore
                  WHERE date <= '{Time.ToScyllaString(date)}'
                  GROUP BY hash, UserId
              ) as Scores
         GROUP BY UserId
     ) ScoreBuilder
ORDER BY global_ranking;
"
        );
        return com;
    }
}