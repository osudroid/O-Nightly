using System.Diagnostics.CodeAnalysis;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class PlayStatsHistory: IPlayStatsHistoryReadonly {
    public required long Id { get; set; }
    public required long PlayId { get; set; }
    public required long Score { get; set; }
    public required DateTime Date { get; set; }
    public required double Pp { get; set; }
    public required long? ReplayFileId { get; set; }

    public PlayStatsHistory() {
    }

    public PlayStatsHistory(PlayStatsHistory clone) {
        PlayId = clone.PlayId;
        Id = clone.Id;
        Score = clone.Score;
        Date = clone.Date;
        Pp = clone.Pp;
        ReplayFileId = clone.ReplayFileId;
    }
    
    public bool IsBetterThen(IPp other) => Pp > other.Pp;

    public static PlayStatsHistory From(IPlayStatsReadonly playStats) {
        return new PlayStatsHistory() {
            Id = -1,
            PlayId = playStats.Id,
            Score = playStats.Score,
            Date = playStats.Date,
            Pp = playStats.Pp,
            ReplayFileId = playStats.ReplayFileId,
        };
    }
}