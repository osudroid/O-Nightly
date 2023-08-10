using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Class;

public class TransformParse<T> : ITransformHandler<T, T> where T : ITransformOutput, IInput {
    public ValueTask<Result<T, string>> Transform(T input) {
        return new ValueTask<Result<T, string>>(Result<T, string>.Ok(input));
    }
}