namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class CreateDropAccountTokenDto : IDto {
    public required string Password { get; init; }
}