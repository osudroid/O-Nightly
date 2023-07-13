using OsuDroid.View;

namespace OsuDroid.Class; 

public record ValidationHandlerResult<TView>(
    Result<ModelResult<TView>, string> View
    ) where TView: IView;