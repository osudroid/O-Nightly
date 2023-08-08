using OsuDroid.View;

namespace OsuDroid.Class;

public record StartTransactionHandlerResult<TView>(
    Result<ModelResult<TView>, string> Result
) where TView : IView;