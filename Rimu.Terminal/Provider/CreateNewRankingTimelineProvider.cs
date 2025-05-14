using System.Collections.Concurrent;
using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Terminal.Provider;

internal class CreateNewRankingTimelineProvider {
    private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryView_Play_PlayStatsHistory _queryView_Play_PlayStatsHistory;

    internal CreateNewRankingTimelineProvider(
        IQueryUserInfo queryUserInfo,
        IQueryView_Play_PlayStatsHistory queryView_Play_PlayStatsHistory) {
        
        _queryUserInfo = queryUserInfo;
        _queryView_Play_PlayStatsHistory = queryView_Play_PlayStatsHistory;
    }

    public async Task RunAsync() {
        var userIds = (await _queryUserInfo.GetAllIdsAsync()).Ok().ToFrozenSet();
        
        Logger.Info("-- Get All Histories");
        var historyWithUser = await GetHistoryAsync();
        foreach (var v in historyWithUser) {
            if (!userIds.Contains(v.UserId)) {
                Logger.Error($"User {v.UserId} does not exist");
                throw new Exception($"User {v.UserId} does not exist");
            }
        }
        
        Logger.Info("Histories Count {}", historyWithUser.Count);
        if (historyWithUser.Count == 0) {
            throw new Exception("No Histories found");
        }
        Logger.Info("-- Filter And Merge Histories");
        var historyWithUserDic = await GetChunkByUserIdAsync(historyWithUser);
        Logger.Info("-- Get Min Max Date From Histories");
        var minDateOnly = historyWithUser.MinBy(x => x.DateOnly)!.DateOnly;
        minDateOnly = minDateOnly > new DateOnly(2021, 1, 1) 
            ? minDateOnly 
            : new DateOnly(2021, 1, 1);
        
        var maxDateOnly = historyWithUser.MaxBy(x => x.DateOnly).DateOnly!;
        
        var dateOnlyList = new List<DateOnly>(365*5);
        
        for (DateOnly dateOnly = minDateOnly; dateOnly <= maxDateOnly; dateOnly = dateOnly.AddDays(1)) {
            dateOnlyList.Add(dateOnly);
        }

        
        foreach (var dateOnlyChunk in dateOnlyList.Chunk(32)) {
            Logger.Debug("Start new Chunk");
            
            try
            {
                await Parallel.ForEachAsync(dateOnlyChunk, async (dateOnly, token) => {
                        await using (var scope = Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.CreateAsyncScope()) {
                            Logger.Info("Calc And Insert From {} -- {}", dateOnly, maxDateOnly);
                            await using var ownDb = await scope.ServiceProvider.GetService<Task<NpgsqlConnection>>()!;

                            var queryGlobalRankingTimeline = scope.ServiceProvider.GetQueryGlobalRankingTimeline();
                            
                            await new CreateRankingTimelineForThisDate(
                                    queryGlobalRankingTimeline, 
                                    historyWithUserDic, 
                                    dateOnly, 
                                    userIds
                            ).RunAsync();
                            
                            Logger.Debug("Calc And Insert From {} -- {} Commit", dateOnly, maxDateOnly);
                        }
                    }
                );
            }
            catch (Exception e) {
                Logger.Error(e);
                throw;
            }
        }
        
        Logger.Info("-- Finished All Insert");
    }

    private async Task<List<SUserIdDateDateOnlyPpPlayId>> GetHistoryAsync() {
        return (await _queryView_Play_PlayStatsHistory
                   .GetAllAsync())
               .Ok()
               .Where(x => x.Pp > 0)
               .Select(SUserIdDateDateOnlyPpPlayId.From)
               .ToList();
    }

    private Task<FrozenDictionary<long, IReadOnlyList<SDateDateOnlyPpPlayId>>> GetChunkByUserIdAsync(
        List<SUserIdDateDateOnlyPpPlayId> histories) {
        
        var dictionary = new Dictionary<long, ConcurrentBag<SDateDateOnlyPpPlayId>>(300_000);
        
        foreach (var view in histories) {
            if (dictionary.ContainsKey(view.UserId)) {
                continue;
            }

            dictionary.Add(view.UserId, new ConcurrentBag<SDateDateOnlyPpPlayId>());
        }

        
        Parallel.ForEach(
            histories,
            history => {
                dictionary[history.UserId].Add(history.ToSDateDateOnlyPpPlayId());   
            }
        );
        
        
        foreach (var key in dictionary.Keys.ToArray()) {
            if (dictionary[key].Count > 0) {
                continue;
            }

            dictionary.Remove(key);
        }
        
        return Task.FromResult(
            dictionary
                .Select(x => new KeyValuePair<long, IReadOnlyList<SDateDateOnlyPpPlayId>>(x.Key, x.Value.ToList()))
                .ToFrozenDictionary());
    }

    private class CreateRankingTimelineForThisDate {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly FrozenDictionary<long, IReadOnlyList<SDateDateOnlyPpPlayId>> _allHistory;
        private readonly DateOnly _dateOnly;
        private readonly FrozenSet<long> _userIds;
        private readonly IQueryGlobalRankingTimeline _queryGlobalRankingTimeline;

        public CreateRankingTimelineForThisDate(
            IQueryGlobalRankingTimeline queryGlobalRankingTimeline,
            FrozenDictionary<long, IReadOnlyList<SDateDateOnlyPpPlayId>> allHistory, 
            DateOnly dateOnly,
            FrozenSet<long> userIds) {
            
            _queryGlobalRankingTimeline = queryGlobalRankingTimeline;
            _allHistory = allHistory;
            _dateOnly = dateOnly;
            _userIds = userIds;
        }

        public async Task RunAsync() {
            var userIdAndPps = CalcTimeLine();

            var globalRankingTimelines = this.CalcGlobalRankingTimeline(userIdAndPps);

            Logger.Info("Calc And Insert From {} -- Insert In DB", _dateOnly);
            
            foreach (var v in globalRankingTimelines.GroupBy(x => x.UserId)
                                                    .Where(x => x.Count() > 1)
                                                    .ToArray()) {
                Logger.Error(v.Key + " has more than one ranking timeline");
                throw new Exception(v.Key + " has more than one ranking timeline");
            }
            
            await InsertToGlobalRankingTimelineAsync(globalRankingTimelines);
        }
        
        private UserIdPpScore[] CalcTimeLine() {
            var result = _allHistory
                         .Select(x => new UserIdPpScore { UserId = x.Key, Pp = default })
                         .ToArray();

            
            Parallel.For(0, result.Length, indexId => {
                UserPpScore userPpScore = default;

                foreach (var v in this._allHistory[result[indexId].UserId]
                                             .Where( x => x.DateOnly <= this._dateOnly)
                                             .GroupBy(x => x.PlayId)
                                             .Select(g => {
                                                 return g.OrderByDescending(x => x.Pp).First();
                                             })) {
                    userPpScore = new UserPpScore(userPpScore.Value + v.Pp);
                }

                result[indexId] = new UserIdPpScore { UserId = result[indexId].UserId, Pp = userPpScore };
            });

            return result.Where(x => x.Pp.Value > 0).ToArray();
        }

        private GlobalRankingTimeline[] CalcGlobalRankingTimeline(UserIdPpScore[] userIdAndPps) {
            var sorted = userIdAndPps.OrderDescending().ToArray();
            var globalRankingTimelines = new GlobalRankingTimeline[sorted.Length];

            for (var indexId = 0; indexId < sorted.Length; indexId++) {
                var v = sorted[indexId];
                globalRankingTimelines[indexId] = new GlobalRankingTimeline() {
                    UserId = v.UserId,
                    Date = new DateTime(this._dateOnly, TimeOnly.MinValue),
                    GlobalRanking = indexId,
                    Pp = v.Pp.Value,
                };
            }
            
            return globalRankingTimelines;
        }

        private async Task InsertToGlobalRankingTimelineAsync(GlobalRankingTimeline[] globalRankingTimelines) {
            foreach (var globalRankingTimeline in globalRankingTimelines) {
                if (!this._userIds.Contains(globalRankingTimeline.UserId)) {
                    Logger.Error($"User {globalRankingTimeline.UserId} does not exist");
                    throw new Exception($"User {globalRankingTimeline.UserId} does not exist");
                }
            }

            foreach (var chunk in globalRankingTimelines.Chunk(10_000)) {
                if ((await _queryGlobalRankingTimeline.InsertBulkAsync(chunk)) == EResult.Err) {
                    throw new Exception("Can Not Insert GlobalRanking Timeline");
                }    
            }
        }
    }

    private record struct SUserIdDateDateOnlyPpPlayId(
        long UserId,
        long PlayId,
        double Pp,
        DateTime DateTime,
        DateOnly DateOnly
    ) {
        public SDateDateOnlyPpPlayId ToSDateDateOnlyPpPlayId() {
            return new (PlayId, Pp, DateTime, DateOnly);
        }

        public static SUserIdDateDateOnlyPpPlayId From(View_Play_PlayStatsHistory v) {
            return new SUserIdDateDateOnlyPpPlayId(
                v.UserId,
                v.PlayId,
                v.Pp,
                v.Date,
                new DateOnly(v.Date.Year, v.Date.Month, v.Date.Day)
            );
        } 
    }

    private record struct SDateDateOnlyPpPlayId(
        long PlayId,
        double Pp,
        DateTime DateTime,
        DateOnly DateOnly
    );
    
    private record struct UserPpScore(double Value) : IComparable<UserPpScore> {
        public int CompareTo(UserPpScore other) {
            return Value.CompareTo(other.Value);
        }
    };
    
    private readonly struct UserIdPpScore : IEquatable<UserIdPpScore>, IComparable<UserIdPpScore> {
        public required UserPpScore Pp { get; init; }
        public required long UserId { get; init; }

        public bool Equals(UserIdPpScore other) {
            return Pp.Equals(other.Pp)
                   && UserId == other.UserId;
        }

        public override bool Equals(object? obj) {
            return obj is UserIdPpScore other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Pp, UserId);
        }

        public int CompareTo(UserIdPpScore other) {
            var ppComparison = Pp.CompareTo(other.Pp);
            if (ppComparison != 0) return ppComparison;
            return UserId.CompareTo(other.UserId);
        }
    }
}