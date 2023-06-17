using OsuDroid.Utils;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2UploadReplayFileDto {
    public required string? MapHash { get; init; }
    public required long ReplayId { get; init; }
}