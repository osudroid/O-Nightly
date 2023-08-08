using OsuDroid.Extensions;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Class; 

public record ControllerPostWrapper<T>(UserCookieControllerHandler Controller, T Post): OsuDroidAttachment.Interface.IInput, ITransformOutput;