
using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdatePatreonEmailDto {
    public required string Email { get; init; }
    public required string Passwd { get; init; }
    public required string Username { get; init; }
}