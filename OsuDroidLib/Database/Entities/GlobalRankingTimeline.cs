using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("GlobalRankingTimeline")]
public class GlobalRankingTimeline {
    [ExplicitKey]
    public long UserId { get; set; }
    [ExplicitKey]
    public DateTime Date { get; set; }
    public long GlobalRanking { get; set; }
    public long Score { get; set; }
}