namespace OsuDroidLib.Query;

public class BblScoreWithUsernameDto {
    public required long PlayScoreId { get; init; }
    public required long UserId { get; init; }
    public required string Username { get; init; }
    public required string EmailHash { get; init; }
    public required string Filename { get; init; }
    public required string Hash { get; init; }
    public required string[] Mode { get; init; }
    public required long Score { get; init; }
    public required long Combo { get; init; }
    public required string Mark { get; init; }
    public required long Geki { get; init; }
    public required long Perfect { get; init; }
    public required long Katu { get; init; }
    public required long Good { get; init; }
    public required long Bad { get; init; }
    public required long Miss { get; init; }
    public required DateTime Date { get; init; }
    public required long Accuracy { get; init; }
}