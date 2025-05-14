using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class PlayStats: IPlayStatsReadonly {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    public required long Id { get; set; }

    public required string[] Mode { get; set; } = Array.Empty<string>();
    public required long Score { get; set; }
    public required long Combo { get; set; }
    public required string Mark { get; set; } = "";
    public required long Geki { get; set; }
    public required long Perfect { get; set; }
    public required long Katu { get; set; }
    public required long Good { get; set; }
    public required long Bad { get; set; }
    public required long Miss { get; set; }
    public required DateTime Date { get; set; }
    public required double Accuracy { get; set; }
    public required double Pp { get; set;  }
    public required long? ReplayFileId { get; set; }

    public PlayStats() {
    }

    public bool IsBetterThen(IPp other) => Pp > other.Pp;
}