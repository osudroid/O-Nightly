using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class WebLoginWithUsernameDto {
    public required int Math { get; init; }
    public required Guid Token { get; init; }
    public required string Username { get; init; }
    public required string Passwd { get; init; }
}