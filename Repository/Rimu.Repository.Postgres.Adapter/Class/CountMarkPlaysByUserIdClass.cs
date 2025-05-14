using Rimu.Repository.Postgres.Adapter.Dto;

namespace Rimu.Repository.Postgres.Adapter.Class;

public class CountMarkPlaysByUserIdClass {
    public long Count { get; }
    public string? NewMark { get; }

    public PlayStatsDto.EPlayScoreMark GetMarkAsEMark() {
        return System.Enum.TryParse(NewMark ?? "", out PlayStatsDto.EPlayScoreMark found)
            ? found
            : throw new Exception("EPlayScoreMark Not Found");
    }
}