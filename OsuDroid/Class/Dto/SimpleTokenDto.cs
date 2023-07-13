namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SimpleTokenDto: IDto {
    public required Guid Token { get; init; }
}