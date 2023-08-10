namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateAvatarDto : IDto {
    public required string ImageBase64 { get; init; }
    public required string Password { get; init; }
}