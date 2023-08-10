using LamLibAllOver;
using OsuDroidAttachment.Class;

namespace OsuDroidAttachment.Interface;

public interface IOutputHandler<TInput, TOutput>
    where TInput : IHandlerOutput {
    public ValueTask<Transaction<TOutput>> Handel(Result<TInput, string> input);
}