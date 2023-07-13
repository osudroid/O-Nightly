using OsuDroidLib.Lib;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class LeaderBoardDto: IDto {
    public required string Region { get; set; }
    public required int Limit { get; set; }

    public Option<CountryInfo.Country> GetRegionAsCountry() {
        return CountryInfo.FindByName(Region ?? "");
    }

    public bool IsRegionAll() {
        return Region == "all";
    }
}