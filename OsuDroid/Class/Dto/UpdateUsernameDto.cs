using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateUsernameDto {
    public required string NewUsername { get; init; }
    public required string OldUsername { get; init; }
    public required string Passwd { get; init; }
}