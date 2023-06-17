
using Patreon.NET;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class CreateApi2TokenDto {
    public required string Username { get; init; }
    public required string Passwd { get; init; }
}