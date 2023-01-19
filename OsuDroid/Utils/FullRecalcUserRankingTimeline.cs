using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Utils;

public static class FullRecalcUserRankingTimeline {
    public static void Run(DateTime startDate) {
        using var mainDb = DbBuilder.BuildPostSqlAndOpen();

        WriteLine("Start FullRecalcUserRankingTimeline");
        WriteLine("DELETE Old Values");
        mainDb.Execute(@"
DELETE FROM public.bbl_global_ranking_timeline
WHERE date >= @0 
", DateTime.SpecifyKind(startDate, DateTimeKind.Utc));

        WriteLine("Get ALl User");
        var userList = CollectionsMarshal.AsSpan(mainDb.Fetch<BblUser>(
                                                     "SELECT id, regist_time FROM public.bbl_user ORDER BY regist_time ASC")
                                                 .OkOrDefault() ??
                                                 new List<BblUser>(0)).ToArray();

        var scoreMapKeyUser = new Dictionary<long, List<BblScore>>();

        foreach (var bblUser in userList) scoreMapKeyUser.Add(bblUser.Id, new List<BblScore>(128));

        WriteLine("Get ALl Score");
        foreach (var bblScore in CollectionsMarshal.AsSpan(mainDb.Fetch<BblScore>(
                                                               "SELECT id, hash, uid, score, date FROM public.bbl_score")
                                                           .OkOrDefault() ??
                                                           new List<BblScore>(0))) {
            if (!scoreMapKeyUser.TryGetValue(bblScore.Uid, out var list)) continue;
            list.Add(bblScore);
        }

        GC.Collect();

        var now = DateTime.UtcNow;
        var endDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, now.Day), DateTimeKind.Utc);


        var dates = new List<DateTime>();

        var iter = startDate;
        while (iter < endDate) {
            dates.Add(iter.Date);
            iter = iter.AddDays(1);
        }

        StrongBox<long> countBox = new(0);

        void MultiIter(int iPosi) {
            Interlocked.Increment(ref countBox.Value);
            var i = dates[iPosi];
            WriteLine($"FROM: {dates.Count} AT: {countBox.Value} | Calc Rank For: Y{i.Year} M{i.Month} D{i.Day}");
            var timeLineArr = GetFullRecalcUserRankingTimelineForThisDay(scoreMapKeyUser, userList, i);
            WriteLine(
                $"FROM: {dates.Count} AT: {countBox.Value} | Insert Values For: Y{i.Year} M{i.Month} D{i.Day} Start");
            using var db = DbBuilder.BuildPostSqlAndOpenNormalPoco();
            db.InsertBatch(timeLineArr);
        }

        Parallel.For(0, dates.Count, new ParallelOptions { MaxDegreeOfParallelism = 13 }, MultiIter);
    }

    private static BblGlobalRankingTimeline[] GetFullRecalcUserRankingTimelineForThisDay(
        Dictionary<long, List<BblScore>> anyScoresKeyUserId, Span<BblUser> users, DateTime end) {
        static (long UserId, long Score) CalcScore(long userId, List<BblScore> bblScores, DateTime end) {
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
            if (bblUser.RegistTime > end) continue;

            if (anyScoresKeyUserId.TryGetValue(bblUser.Id, out var bblScores) == false) continue;

            var (userId, score) = CalcScore(bblUser.Id, bblScores, end);
            userAndScoreSumsList.Add(new UserIdAndScoreSum(userId, score));
        }

        var userAndScoreSumsArr = userAndScoreSumsList.Where(x => x.Score > 0).ToArray();

        Array.Sort(userAndScoreSumsArr,
            delegate(UserIdAndScoreSum x, UserIdAndScoreSum y) { return x.Score.CompareTo(y.Score); });

        var res = new BblGlobalRankingTimeline[userAndScoreSumsArr.Length];

        for (var i = 0; i < res.Length; i++) {
            var v = userAndScoreSumsArr[i];
            res[i] = new BblGlobalRankingTimeline {
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