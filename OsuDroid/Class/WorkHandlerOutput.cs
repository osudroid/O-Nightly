using OsuDroidAttachment.Interface;

namespace OsuDroid.Class; 

public struct WorkHandlerOutput : IHandlerOutput {
    public bool Work { get; private init; }

    public static WorkHandlerOutput False => new WorkHandlerOutput { Work = false };
    public static WorkHandlerOutput True => new WorkHandlerOutput { Work = true };
}