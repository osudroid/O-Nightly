using LamLibAllOver.ErrorHandling;
using MediatR;
using Rimu.Repository.Attachment.Adapter.Class;

namespace Rimu.Repository.Attachment.Adapter.Interface;

public interface IAttachmentServiceApi<HandlerInput, HandlerOutput>: IDisposable, IAsyncDisposable
    where HandlerInput: MediatR.IRequest<bool>, IRequest<SResult<OptionHandlerOutput<HandlerOutput>>> {
    public Task<ITransaction<HandlerOutput>> RunOperationAsync(HandlerInput handlerInput, CancellationToken token);
}