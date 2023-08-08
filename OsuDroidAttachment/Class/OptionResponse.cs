using LamLibAllOver;

namespace OsuDroidAttachment.Class; 

public record OptionResponse<T>(Option<T> Data) {
    public static OptionResponse<T> Empty() => new(Option<T>.Empty);
    public static OptionResponse<T> Set(T data) => new(Option<T>.With(data));
}