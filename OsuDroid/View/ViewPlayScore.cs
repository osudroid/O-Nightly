namespace OsuDroid.Class;

public sealed class ViewPlayScore {
    public long PlayScoreId { get; set; }
    public long UserId { get; set; }
    public string? Filename { get; set; }
    public string? Hash { get; set; }
    public string[]? Mode { get; set; }
    public long Score { get; set; }
    public long Combo { get; set; }
    public string? Mark { get; set; }
    public long Geki { get; set; }
    public long Perfect { get; set; }
    public long Katu { get; set; }
    public long Good { get; set; }
    public long Bad { get; set; }
    public long Miss { get; set; }
    public DateTime Date { get; set; }
    public long Accuracy { get; set; }

    public static ViewPlayScore FromPlayScore(Entities.PlayScore playScore) {
        return new() {
            PlayScoreId = playScore.PlayScoreId,
            UserId = playScore.UserId,
            Filename = playScore.Filename,
            Hash = playScore.Hash,
            Mode = playScore.Mode,
            Score = playScore.Score,
            Combo = playScore.Combo,
            Mark = playScore.Mark,
            Geki = playScore.Geki,
            Perfect = playScore.Perfect,
            Katu = playScore.Katu,
            Good = playScore.Good,
            Bad = playScore.Bad,
            Miss = playScore.Miss,
            Date = playScore.Date,
            Accuracy = playScore.Accuracy,
        };
    }
}