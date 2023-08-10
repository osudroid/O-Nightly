namespace OsuDroid.Class.Dto;

// ReSharper disable All
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2UploadReplayFileDto : IDto {
    public required string? MapHash { get; init; }
    public required long ReplayId { get; init; }
}