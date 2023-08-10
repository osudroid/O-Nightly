using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.Interface;

namespace OsuDroid.OutputHandler;

public struct
    ViewExistOrFoundInfoHandler<T> : IOutputHandler<OptionHandlerOutput<T>, ApiTypes.ViewExistOrFoundInfo<T>> {
    public ValueTask<Transaction<ApiTypes.ViewExistOrFoundInfo<T>>>
        Handel(Result<OptionHandlerOutput<T>, string> input) {
        if (input == EResult.Err)
            return new ValueTask<Transaction<ApiTypes.ViewExistOrFoundInfo<T>>>(
                Transaction<ApiTypes.ViewExistOrFoundInfo<T>>.InternalServerError(input)
            );

        var option = input.Ok().Option;

        return new ValueTask<Transaction<ApiTypes.ViewExistOrFoundInfo<T>>>(
            option.IsNotSet()
                ? Transaction<ApiTypes.ViewExistOrFoundInfo<T>>.Ok(ApiTypes.ViewExistOrFoundInfo<T>.NotExist())
                : Transaction<ApiTypes.ViewExistOrFoundInfo<T>>.Ok(
                    ApiTypes.ViewExistOrFoundInfo<T>.Exist(option.Unwrap())
                )
        );
    }
}