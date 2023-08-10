namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdatePasswdDto : IDto {
    public required string NewPassword { get; init; }
    public required string OldPassword { get; init; }
}