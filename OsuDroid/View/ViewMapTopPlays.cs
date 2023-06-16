using OsuDroidLib.Query;

namespace OsuDroid.View; 

public class ViewMapTopPlays {
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

    public static ViewMapTopPlays FromMapTopPlays(QueryPlayScore.MapTopPlays mapTopPlays) {
        return new() {
            PlayScoreId = mapTopPlays.PlayScoreId,
            UserId = mapTopPlays.UserId,
            Mode = mapTopPlays.Mode,
            Score = mapTopPlays.Score,
            Combo = mapTopPlays.Combo,
            Mark = mapTopPlays.Mark,
            Date = mapTopPlays.Date,
            Accuracy = mapTopPlays.Accuracy,
            Username = mapTopPlays.Username,
            PlayRank = mapTopPlays.PlayRank,
        };
    } 
}