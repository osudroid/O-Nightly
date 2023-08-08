using OsuDroidAttachment.Interface;

namespace OsuDroid.HttpGet; 

public struct GetAvatar: IInput {
    public required int Size { get; init; }
    public required long Id { get; init; }
}