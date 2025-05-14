namespace Rimu.Repository.Postgres.Adapter.Dto;

public class TopPlaysByMarkPageingDto {
    public required long UserId { get; init; }
    public required PlayStatsDto.EPlayScoreMark MarkString { get; init; }
    public required int Page { get; init; }
}