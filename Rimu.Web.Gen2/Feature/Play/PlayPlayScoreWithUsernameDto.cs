using Rimu.Repository.Postgres.Adapter.Dto;

namespace Rimu.Web.Gen2.Feature.Play;

public class PlayPlayScoreWithUsernameDto {
    public required PlayPlayStatsDto PlayPlayStatsDto { get; set; }
    public required string Username { get; set; }
}