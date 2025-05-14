using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Dto;

public struct View_Play_PlayStats_Dto {
    public required PlayStatsDto PlayStats { get; init; }
    public required PlayDto Play { get; init; }

    public static View_Play_PlayStats_Dto From(IPlay_PlayStatsReadonly playPlayStats) {
        return new View_Play_PlayStats_Dto() {
            Play = PlayDto.Create(playPlayStats),
            PlayStats = PlayStatsDto.Create(playPlayStats)
        };
    }
}