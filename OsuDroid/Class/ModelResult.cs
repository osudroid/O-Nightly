using OsuDroid.View;

namespace OsuDroid.Class;

public interface IModelResult {
    EModelResult Mode { get; }
}

public record ModelResult<T>(Option<T> Result, EModelResult Mode) : IModelResult where T : IView {
    public static ModelResult<T> Ok(T result) {
        return new ModelResult<T>(Option<T>.With(result), EModelResult.Ok);
    }

    public static ModelResult<T> BadRequest() {
        return new ModelResult<T>(Option<T>.Empty, EModelResult.BadRequest);
    }

    public static ModelResult<T> InternalServerError() {
        return new ModelResult<T>(Option<T>.Empty, EModelResult.InternalServerError);
    }
}