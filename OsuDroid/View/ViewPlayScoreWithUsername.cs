namespace OsuDroid.View;

public class ViewPlayScoreWithUsername: IView {
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
    public string? Username { get; set; }

    public static ViewPlayScoreWithUsername FromPlayScoreWithUsername(
        Entities.PlayScoreWithUsername playScoreWithUsername) {
        return new() {
            PlayScoreId = playScoreWithUsername.PlayScoreId,
            UserId = playScoreWithUsername.UserId,
            Filename = playScoreWithUsername.Filename,
            Hash = playScoreWithUsername.Hash,
            Mode = playScoreWithUsername.Mode,
            Score = playScoreWithUsername.Score,
            Combo = playScoreWithUsername.Combo,
            Mark = playScoreWithUsername.Mark,
            Geki = playScoreWithUsername.Geki,
            Perfect = playScoreWithUsername.Perfect,
            Katu = playScoreWithUsername.Katu,
            Good = playScoreWithUsername.Good,
            Bad = playScoreWithUsername.Bad,
            Miss = playScoreWithUsername.Miss,
            Date = playScoreWithUsername.Date,
            Accuracy = playScoreWithUsername.Accuracy,
            Username = playScoreWithUsername.Username,
        };
    }
}