using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Dto;

public struct PlayDto: IPlayReadonly {
    public required long Id { get; init; }
    public required long UserId { get; init; }
    public required string Filename { get; init; }
    public required string FileHash { get; init; }

    public static PlayDto Create(IPlayReadonly play) {
        return new PlayDto() {
            Id = play.Id,
            UserId = play.UserId,
            Filename = play.Filename,
            FileHash = play.FileHash,
        };
    }
}