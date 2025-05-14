// using System.Runtime.CompilerServices;
// using Dapper;
// using LamLibAllOver;
// using NLog;
// using Rimu.Database.Builder;
// using Rimu.DB.Entity;
// using Rimu.DB.Entity.V2;
// using Logger = NLog.Logger;
//
// namespace Rimu.Terminal;
//
// public static class FullRecalcUserRankingTimeline {
//     private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
//     
//     public static void Run(DateTime startDate) {
//         RunAsync(startDate).Wait();
//     }
//
//     private static async Task RunAsync(DateTime startDate) {
//         var v = GC.TryStartNoGCRegion(14884901888, 13884901888, true);
//         
//         var iter = startDate;
//         var dates = new List<DateTime>();
//         var scoreMapKeyUser = new Dictionary<long, List<PlayScoreNOTUSE>>();
//         UserInfo[] userList;
//
//         await using var mainDb = await DbBuilder.BuildNpgsqlConnection();
//
//         Logger.Info("Start FullRecalcUserRankingTimeline");
//         Logger.Info("DELETE Old Values");
//         var dateStr = $"{startDate.Year}-{startDate.Month}-{startDate.Day}";
//         await mainDb.SafeQueryAsync(@$"
// DELETE FROM public.GlobalRankingTimeline
// WHERE date >= '{dateStr}' 
// "
//         );
//
//
//         Logger.Info("Get ALl User");
//         userList = (await mainDb.SafeQueryAsync<UserInfo>(
//                 "SELECT UserId, RegisterTime FROM UserInfo ORDER BY RegisterTime ASC"
//             )).Ok()
//               .ToArray();
//
//
//         foreach (var bblUser in userList) {
//             scoreMapKeyUser.Add(bblUser.UserId, new List<PlayScoreNOTUSE>(128));
//         }
//
//         Logger.Info("Get ALl Score");
//         foreach (var bblScore in (await mainDb.SafeQueryAsync<PlayScoreNOTUSE>(
//                      "SELECT PlayScoreId, Hash, UserId, Score, Date FROM public.PlayScore"
//                  )).Ok()
//                    .ToList()) {
//             if (!scoreMapKeyUser.TryGetValue(bblScore.UserId, out var list)) continue;
//             list.Add(bblScore);
//         }
//
//         GC.Collect();
//
//         var now = DateTime.UtcNow;
//         var endDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
//
//         while (iter <= endDate) {
//             dates.Add(iter.Date);
//             iter = iter.AddDays(1);
//         }
//
//
//         StrongBox<long> countCountBox = new(0);
//
//         async ValueTask MultiIter(int iPosi, CancellationToken cancellation) {
//             try {
//                 var count = Interlocked.Increment(ref countCountBox.Value);
//                 var i = dates[iPosi];
//                 Logger.Info($"FROM: {dates.Count} AT: {count} | Calc Rank For: Y{i.Year} M{i.Month} D{i.Day}");
//                 GlobalRankingTimeline[] timeLineArr = GetFullRecalcUserRankingTimelineForThisDay(scoreMapKeyUser, userList, i);
//                 Logger.Info(
//                     $"FROM: {dates.Count} AT: {count} | Insert Values For: Y{i.Year} M{i.Month} D{i.Day} Start"
//                 );
//                 await using var db = await DbBuilder.BuildNpgsqlConnection();
//
//
//                 // var lines = string.Join(
//                 //     ", ",
//                 //     timeLineArr.Select(
//                 //         x => $"({x.UserId}, {Time.ToScyllaString(x.Date)}, {x.GlobalRanking}, {x.Score})"
//                 //     )
//                 // );
//                 await db.ExecuteAsync(@$"
// INSERT INTO GlobalRankingTimeline
// (Userid, Date, Globalranking, Score) 
// VALUES
// (
//  @UserId,
//  @Date,
//  @GlobalRanking,
//  @Score
// )
// ", timeLineArr
//                   );
//             }
//             catch (Exception e) {
//                 Logger.Error(e);
//                 throw;
//             }
//         }
//
//         await Parallel.ForAsync(0, dates.Count,
//             new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount + 2 }, 
//             MultiIter
//         );
//     }
//
//     private static GlobalRankingTimeline[] GetFullRecalcUserRankingTimelineForThisDay(
//         Dictionary<long, List<PlayScoreNOTUSE>> anyScoresKeyUserId,
//         Span<UserInfo> users,
//         DateTime end) {
//         static (long UserId, long Score) CalcScore(long userId, List<PlayScoreNOTUSE> bblScores, DateTime end) {
//             var scoreMapUser = new Dictionary<string, long>(32);
//
//             foreach (var bblScore in bblScores) {
//                 if (bblScore.Date > end) continue;
//
//                 if (!scoreMapUser.ContainsKey(bblScore.Hash!)) {
//                     scoreMapUser.Add(bblScore.Hash!, bblScore.Score);
//                     continue;
//                 }
//
//                 var fromMapScore = scoreMapUser[bblScore.Hash!];
//                 if (fromMapScore >= bblScore.Score) continue;
//
//                 scoreMapUser[bblScore.Hash!] = bblScore.Score;
//             }
//
//             long score = 0;
//
//             foreach (var (s, value) in scoreMapUser) score += value;
//
//             return (userId, score);
//         }
//
//         var userAndScoreSumsList = new List<UserIdAndScoreSum>(users.Length);
//
//         foreach (var bblUser in users) {
//             if (bblUser.RegisterTime > end) continue;
//
//             if (anyScoresKeyUserId.TryGetValue(bblUser.UserId, out var bblScores) == false) continue;
//
//             var (userId, score) = CalcScore(bblUser.UserId, bblScores, end);
//             userAndScoreSumsList.Add(new UserIdAndScoreSum(userId, score));
//         }
//
//         var userAndScoreSumsArr = userAndScoreSumsList.Where(x => x.Score > 0).ToArray();
//
//         Array.Sort(userAndScoreSumsArr,
//             delegate(UserIdAndScoreSum x, UserIdAndScoreSum y) { return x.Score.CompareTo(y.Score); }
//         );
//
//         var res = new GlobalRankingTimeline[userAndScoreSumsArr.Length];
//
//         for (var i = 0; i < res.Length; i++) {
//             var v = userAndScoreSumsArr[i];
//             res[i] = new GlobalRankingTimeline {
//                 UserId = v.UserId,
//                 Date = DateTime.SpecifyKind(end, DateTimeKind.Utc),
//                 Score = v.Score,
//                 GlobalRanking = userAndScoreSumsArr.Length - i
//             };
//         }
//
//         return res;
//     }
//
//     private readonly struct UserIdAndScoreSum : IComparable {
//         public readonly long UserId;
//         public readonly long Score;
//
//         public UserIdAndScoreSum(long userId, long score) {
//             UserId = userId;
//             Score = score;
//         }
//
//         public int CompareTo(object? obj) {
//             if (obj is null) return Score.CompareTo(0);
//             return Score.CompareTo(((UserIdAndScoreSum)obj).Score);
//         }
//     }
// }