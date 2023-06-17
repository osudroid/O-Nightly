using OsuDroid.Lib.Validate;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class WebRegisterDto {
    public required string Email { get; init; }
    public required int MathRes { get; init; }
    public required Guid MathToken { get; init; }
    public required string Region { get; init; }
    public required string Passwd { get; init; }
    public required string Username { get; init; }
}