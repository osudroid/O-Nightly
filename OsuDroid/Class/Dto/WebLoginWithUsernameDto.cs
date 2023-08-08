using OsuDroidAttachment.Interface;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class WebLoginWithUsernameDto: IDto, ITransformOutput {
    public required int Math { get; init; }
    public required Guid Token { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}