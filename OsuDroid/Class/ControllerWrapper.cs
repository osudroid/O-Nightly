using OsuDroidAttachment.Interface;

namespace OsuDroid.Class;

public record struct ControllerWrapper(UserCookieControllerHandler Controller) : IInput, ITransformOutput;