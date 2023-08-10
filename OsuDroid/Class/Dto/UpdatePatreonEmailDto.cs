namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdatePatreonEmailDto : IDto {
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Username { get; init; }
}