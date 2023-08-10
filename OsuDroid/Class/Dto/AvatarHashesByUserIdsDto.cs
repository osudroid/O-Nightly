namespace OsuDroid.Class.Dto;

// ReSharper disable All
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AvatarHashesByUserIdsDto : IDto {
    public required int Size { get; init; }
    public required long[] UserIds { get; init; }
}