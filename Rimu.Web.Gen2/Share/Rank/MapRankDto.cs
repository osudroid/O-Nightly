using Rimu.Repository.Postgres.Adapter.Dto;

namespace Rimu.Web.Gen2.Share.Rank;

public sealed class MapRankDto {
    public required string Username { get; set; }
    public required long Rank { get; set; }
    public required View_Play_PlayStats_Dto Stat { get; set; }
}