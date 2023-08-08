using OsuDroidAttachment.Interface;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class WebLoginDto : ITransformOutput {
    public required int Math { get; init; }
    public required Guid Token { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}