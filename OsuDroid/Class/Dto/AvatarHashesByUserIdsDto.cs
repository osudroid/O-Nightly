namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AvatarHashesByUserIdsDto {
    public required int Size { get; init; }
    public required long[] UserIds { get; init; }
}