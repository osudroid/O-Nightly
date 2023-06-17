using OsuDroid.Lib.Validate;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateAvatarDto {
    public required string ImageBase64 { get; init; }
    public required string Passwd { get; init; }
}