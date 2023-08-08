namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdatePasswdDto : IDto {
    public required string NewPasswd { get; init; }
    public required string OldPasswd { get; init; }
}