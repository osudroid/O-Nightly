namespace OsuDroid.Class.Dto;

// ReSharper disable All
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2PlayByIdDto : IDto {
    public required long PlayId { get; init; }
}