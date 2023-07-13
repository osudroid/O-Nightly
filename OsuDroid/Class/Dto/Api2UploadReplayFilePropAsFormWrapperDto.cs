namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2UploadReplayFilePropAsFormWrapperDto: IDto {
    public required IFormFile? File { get; init; }
    public required string? Prop { get; init; }
}