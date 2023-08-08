using OsuDroid.Extensions;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Class; 

public record ControllerGetWrapper<T>(UserCookieControllerHandler Controller, T Get): OsuDroidAttachment.Interface.IInput, ITransformOutput;