using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Class;

public record struct OptionHandlerOutput<T>(Option<T> Option) : IHandlerOutput {
    public static OptionHandlerOutput<T> Empty => default;

    public static OptionHandlerOutput<T> With(T value) {
        return new OptionHandlerOutput<T>(Option<T>.With(value));
    }
}