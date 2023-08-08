using OsuDroid.Extensions;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Class; 

public record struct ControllerWrapper(UserCookieControllerHandler Controller): OsuDroidAttachment.Interface.IInput, ITransformOutput;