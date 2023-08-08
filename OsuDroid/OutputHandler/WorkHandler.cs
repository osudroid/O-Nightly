using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.Interface;

namespace OsuDroid.OutputHandler;

public struct WorkHandler : IOutputHandler<WorkHandlerOutput, ApiTypes.ViewWork> {
    public ValueTask<Transaction<ApiTypes.ViewWork>> Handel(Result<WorkHandlerOutput, string> input) {
        if (input == EResult.Err)
            return new ValueTask<Transaction<ApiTypes.ViewWork>>(
                Transaction<ApiTypes.ViewWork>.InternalServerError(input));
        return new ValueTask<Transaction<ApiTypes.ViewWork>>(
            input.Ok().Work
                ? Transaction<ApiTypes.ViewWork>.Ok(ApiTypes.ViewWork.False)
                : Transaction<ApiTypes.ViewWork>.Ok(ApiTypes.ViewWork.True)
        );
    }
}