namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class WebLoginDto: IDto {
    public required int Math { get; init; }
    public required Guid Token { get; init; }
    public required string Email { get; init; }
    public required string Passwd { get; init; }
}