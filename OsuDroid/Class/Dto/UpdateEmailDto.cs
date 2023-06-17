using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateEmailDto {
    public required string NewEmail { get; init; }
    public required string OldEmail { get; init; }
    public required string Passwd { get; init; }
}