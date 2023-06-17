using OsuDroid.Utils;
using OsuDroid.Class;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class RecentPlaysDto {
    public required string FilterPlays { get; init; }
    public required string OrderBy     { get; init; }
    public required int    Limit       { get; init; }
    public required int    StartAt     { get; init; }
}