namespace Rimu.Repository.Postgres.Adapter.Class;

public class MapTopPlays {
    public long PlayScoreId { get; set; }
    public long UserId { get; set; }
    public string? Mode { get; set; }
    public long Score { get; set; }
    public long Combo { get; set; }
    public string? Mark { get; set; }
    public DateTime? Date { get; set; }
    public long Accuracy { get; set; }
    public string? Username { get; set; }
    public long PlayRank { get; set; }
}