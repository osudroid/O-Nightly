using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment; 

public class TransformAction<TInput, EOutput>
    (Func<TInput, EOutput> fun) : ITransformHandler<TInput, EOutput>
    where TInput : IInput
    where EOutput: ITransformOutput {

    public ValueTask<Result<EOutput, string>> Transform(TInput input) {
        try {
            return ValueTask.FromResult(Result<EOutput, string>.Ok(fun(input)));
        }
        catch (Exception e) {
            return ValueTask.FromResult(Result<EOutput, string>.Err(e.ToString()));
        }
    }
}