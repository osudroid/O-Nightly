using System.Runtime.CompilerServices;
using Dapper;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroid.Utils;

public static class FullRecalcUserRankingTimeline {
    public static void Run(DateTime startDate) {
        RunAsync(startDate).Wait();
    }

    private static async Task RunAsync(DateTime startDate) {
        var iter = startDate;
        var dates = new List<DateTime>();
        var scoreMapKeyUser = new Dictionary<long, List<PlayScore>>();
        UserInfo[] userList;

        await using var mainDb = await DbBuilder.BuildNpgsqlConnection();

        WriteLine("Start FullRecalcUserRankingTimeline");
        WriteLine("DELETE Old Values");
        var dateStr = $"{startDate.Year}-{startDate.Month}-{startDate.Day}";
        await mainDb.SafeQueryAsync(@$"
DELETE FROM public.GlobalRankingTimeline
WHERE date >= '{dateStr}' 
");


        WriteLine("Get ALl User");
        userList = (await mainDb.SafeQueryAsync<UserInfo>(
            "SELECT UserId, RegisterTime FROM UserInfo ORDER BY RegisterTime ASC")).Ok().ToArray();


        foreach (var bblUser in userList) scoreMapKeyUser.Add(bblUser.UserId, new List<PlayScore>(128));

        WriteLine("Get ALl Score");
        foreach (var bblScore in (await mainDb.SafeQueryAsync<PlayScore>(
                     "SELECT PlayScoreId, Hash, UserId, Score, Date FROM public.PlayScore")).Ok().ToList()) {
            if (!scoreMapKeyUser.TryGetValue(bblScore.UserId, out var list)) continue;
            list.Add(bblScore);
        }

        GC.Collect();

        var now = DateTime.UtcNow;
        var endDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, System.DateTimeKind.Utc);

        while (iter <= endDate) {
            dates.Add(iter.Date);
            iter = iter.AddDays(1);
        }


        StrongBox<long> countCountBox = new(0);

        void MultiIter(int iPosi) {
            try {
                var count = Interlocked.Increment(ref countCountBox.Value);
                var i = dates[iPosi];
                WriteLine($"FROM: {dates.Count} AT: {count} | Calc Rank For: Y{i.Year} M{i.Month} D{i.Day}");
                var timeLineArr = GetFullRecalcUserRankingTimelineForThisDay(scoreMapKeyUser, userList, i);
                WriteLine(
                    $"FROM: {dates.Count} AT: {count} | Insert Values For: Y{i.Year} M{i.Month} D{i.Day} Start");
                using var db = DbBuilder.BuildNpgsqlConnection().GetAwaiter().GetResult();


                var lines = String.Join(
                    ", ",
                    timeLineArr.Select(
                        x => $"({x.UserId}, {Time.ToScyllaString(x.Date)}, {x.GlobalRanking}, {x.Score})")
                );
                db.QueryAsync(@$"
INSERT INTO GlobalRankingTimeline
(Userid, Date, Globalranking, Score) 
VALUES
{lines}
").Wait();
            }
            catch (Exception e) {
                WriteLine(e);
                throw;
            }
        }

        Parallel.For(0, dates.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount + 2 },
            MultiIter);
    }

    private static GlobalRankingTimeline[] GetFullRecalcUserRankingTimelineForThisDay(
        Dictionary<long, List<PlayScore>> anyScoresKeyUserId, Span<UserInfo> users, DateTime end) {
        static (long UserId, long Score) CalcScore(long userId, List<PlayScore> bblScores, DateTime end) {
            var scoreMapUser = new Dictionary<string, long>(32);

            foreach (var bblScore in bblScores) {
                if (bblScore.Date > end) continue;

                if (!scoreMapUser.ContainsKey(bblScore.Hash!)) {
                    scoreMapUser.Add(bblScore.Hash!, bblScore.Score);
                    continue;
                }

                var fromMapScore = scoreMapUser[bblScore.Hash!];
                if (fromMapScore >= bblScore.Score) continue;

                scoreMapUser[bblScore.Hash!] = bblScore.Score;
            }

            long score = 0;

            foreach (var (s, value) in scoreMapUser) score += value;

            return (userId, score);
        }

        var userAndScoreSumsList = new List<UserIdAndScoreSum>(users.Length);

        foreach (var bblUser in users) {
            if (bblUser.RegisterTime > end) continue;

            if (anyScoresKeyUserId.TryGetValue(bblUser.UserId, out var bblScores) == false) continue;

            var (userId, score) = CalcScore(bblUser.UserId, bblScores, end);
            userAndScoreSumsList.Add(new UserIdAndScoreSum(userId, score));
        }

        var userAndScoreSumsArr = userAndScoreSumsList.Where(x => x.Score > 0).ToArray();

        Array.Sort(userAndScoreSumsArr,
            delegate(UserIdAndScoreSum x, UserIdAndScoreSum y) { return x.Score.CompareTo(y.Score); });

        var res = new GlobalRankingTimeline[userAndScoreSumsArr.Length];

        for (var i = 0; i < res.Length; i++) {
            var v = userAndScoreSumsArr[i];
            res[i] = new GlobalRankingTimeline {
                UserId = v.UserId,
                Date = DateTime.SpecifyKind(end, DateTimeKind.Utc),
                Score = v.Score,
                GlobalRanking = userAndScoreSumsArr.Length - i
            };
        }

        return res;
    }

    private readonly struct UserIdAndScoreSum : IComparable {
        public readonly long UserId;
        public readonly long Score;

        public UserIdAndScoreSum(long userId, long score) {
            UserId = userId;
            Score = score;
        }

        public int CompareTo(object? obj) {
            if (obj is null) return Score.CompareTo(0);
            return Score.CompareTo(((UserIdAndScoreSum)obj).Score);
        }
    }
}