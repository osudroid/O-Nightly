using OsuDroidLib.Lib;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class LeaderBoardSearchUserDto : IDto {
    public required string Region { get; init; }
    public required long Limit { get; init; }
    public required string Query { get; init; }

    public Option<CountryInfo.Country> GetRegionAsCountry() {
        return CountryInfo.FindByName(Region);
    }

    public bool IsRegionAll() {
        return Region == "all";
    }
}