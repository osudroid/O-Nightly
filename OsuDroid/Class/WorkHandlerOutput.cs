using OsuDroidAttachment.Interface;

namespace OsuDroid.Class;

public struct WorkHandlerOutput : IHandlerOutput {
    public bool Work { get; private init; }

    public static WorkHandlerOutput False => new() { Work = false };
    public static WorkHandlerOutput True => new() { Work = true };
}