namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2PlayByIdDto : IDto {
    public required long PlayId { get; init; }
}