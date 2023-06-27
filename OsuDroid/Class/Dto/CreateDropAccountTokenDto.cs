using OsuDroid.Class;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class CreateDropAccountTokenDto {
    public required string Password { get; init; }
}