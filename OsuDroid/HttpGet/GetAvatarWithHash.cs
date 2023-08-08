using OsuDroidAttachment.Interface;

namespace OsuDroid.HttpGet; 

public struct GetAvatarWithHash: IInput {
    public required string Hash { get; init; }
}