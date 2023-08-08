namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateEmailDto : IDto {
    public required string NewEmail { get; init; }
    public required string OldEmail { get; init; }
    public required string Passwd { get; init; }
}