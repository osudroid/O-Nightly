namespace OsuDroid.Class;

public record ModelResult<T>(Option<T> Result, EModelResult Mode) {
    public static ModelResult<T> Ok(T result) => new(Option<T>.With(result), EModelResult.Ok);
    public static ModelResult<T> BadRequest() => new(Option<T>.Empty, EModelResult.BadRequest);
    public static ModelResult<T> InternalServerError() => new(Option<T>.Empty, EModelResult.InternalServerError);
}