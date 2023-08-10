namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateUsernameDto : IDto {
    public required string NewUsername { get; init; }
    public required string OldUsername { get; init; }
    public required string Password { get; init; }
}