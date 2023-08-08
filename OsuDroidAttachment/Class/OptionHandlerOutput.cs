using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Class;

public record struct OptionHandlerOutput<T>(Option<T> Option) : IHandlerOutput {
    public static OptionHandlerOutput<T> Empty => default;
    public static OptionHandlerOutput<T> With(T value) => new(Option<T>.With(value)); 
}