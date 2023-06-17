using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdatePasswdDto  {
    public required string NewPasswd { get; init; }
    public required string OldPasswd { get; init; }
}