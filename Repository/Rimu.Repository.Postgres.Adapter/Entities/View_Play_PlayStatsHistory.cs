using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class View_Play_PlayStatsHistory: IPlay_PlayStatsHistoryReadonly {
    public required long Id { get; set; } 
    public required long PlayId { get; set; } 
    public required long Score { get; set; } 
    public required DateTime Date { get; set; } 
    public required long UserId { get; set; } 
    public required string FileHash { get; set; } = "";
    public required string Filename { get; set; } = "";
    public required double Pp { get; set; }
    public required long? ReplayFileId { get; set; }
    public bool IsBetterThen(IPp other) => Pp > other.Pp;
}