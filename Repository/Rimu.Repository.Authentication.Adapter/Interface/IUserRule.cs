namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IUserRule {
    public bool IsRestrict { get; }
    public bool IsBanned { get; }
    public bool IsArchived { get; }
    public bool Login  { get; }
    public bool Multiplayer  { get; }
    public bool ScoreSubmission  { get; }
    public bool GlobalRanking  { get; }
    public bool BeatmapRanking  { get; }
    public bool ProfilePageAccess  { get; }
}