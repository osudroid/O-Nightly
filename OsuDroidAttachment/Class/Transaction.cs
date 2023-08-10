using LamLibAllOver;

namespace OsuDroidAttachment.Class;

public record Transaction<T>(
    EModelResult Result,
    Option<T> OptionResponse,
    ResultErr<string> Err,
    string UserErrorMessage = "") {
    public static Transaction<T> Ok(T result) {
        return new Transaction<T>(EModelResult.Ok, Option<T>.With(result), ResultErr<string>.Ok());
    }

    public static Transaction<T> BadRequest() {
        return new Transaction<T>(EModelResult.BadRequest, Option<T>.Empty, ResultErr<string>.Ok());
    }

    public static Transaction<T> InternalServerError(ResultErr<string> resultErr) {
        return new Transaction<T>(EModelResult.InternalServerError, Option<T>.Empty, resultErr);
    }

    public static Transaction<T> HashNotMatch(string err) {
        return new Transaction<T>(EModelResult.BadRequestWithMessage, Option<T>.Empty, ResultErr<string>.Ok(), err);
    }
}