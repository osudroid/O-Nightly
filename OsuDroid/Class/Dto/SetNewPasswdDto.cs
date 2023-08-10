namespace OsuDroid.Class.Dto;

public sealed class SetNewPasswdDto : IDto {
    public required string NewPassword { get; init; }
    public required string Token { get; init; }
    public required long UserId { get; init; }
}