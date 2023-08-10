using LamLibAllOver;

namespace OsuDroidAttachment.Class;

public record OptionResponse<T>(Option<T> Data) {
    public static OptionResponse<T> Empty() {
        return new OptionResponse<T>(Option<T>.Empty);
    }

    public static OptionResponse<T> Set(T data) {
        return new OptionResponse<T>(Option<T>.With(data));
    }
}