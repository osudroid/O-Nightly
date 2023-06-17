using OsuDroid.Lib.Validate;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ResetPasswdAndSendEmailDto {
    public required string Email { get; init; }
    public required string Username { get; init; }
}