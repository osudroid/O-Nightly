using LamLibAllOver;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model;

public record OptionResponse<T>(Option<T> Data) : IResponse where T : IDto {
    public static OptionResponse<T> Empty() => new(Option<T>.Empty);
    public static OptionResponse<T> Set(T data) => new(Option<T>.With(data));
}