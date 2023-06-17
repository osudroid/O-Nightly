using OsuDroid.Utils;
using OsuDroid.Class;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2PlayByIdDto {
    public required long PlayId { get; init; }
}