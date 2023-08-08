using OsuDroidAttachment.Interface;

namespace OsuDroid.Class;

public record ControllerGetWrapper<T>(UserCookieControllerHandler Controller, T Get) : IInput, ITransformOutput;