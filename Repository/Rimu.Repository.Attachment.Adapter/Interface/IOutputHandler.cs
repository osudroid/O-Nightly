using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Attachment.Adapter.Interface;

public interface IOutputHandler<TInput, TOutput>
    where TInput : IHandlerOutput {
    public ValueTask<ITransaction<TOutput>> Handel(SResult<TInput> input);
}

