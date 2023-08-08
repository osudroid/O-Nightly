using Microsoft.Extensions.Logging;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment; 

public class AttachmentServiceApiBuilder<TDb, ELogger, UInput, TransformOutput, HandlerOutput, Output>
    where TDb: IDb 
    where ELogger: OsuDroidAttachment.Interface.ILogger
    where UInput: IInput 
    where TransformOutput: ITransformOutput
    where HandlerOutput: IHandlerOutput {
    
    public required IDbCreates<TDb> DbCreates { internal get; init; }
    public required ILoggerCreates<TDb, ELogger> LoggerCreates { internal get; init; }
    public required IValidationHandler<TDb, ELogger, UInput> ValidationHandler { internal get; init; }
    public required ITransformHandler<UInput,TransformOutput> TransformHandler { internal get; init; }
    public required IHandler<TDb, ELogger, TransformOutput, HandlerOutput> Handler { internal get; init; }
    public required IOutputHandler<HandlerOutput, Output> OutputHandler { internal get; init; }

    public (
        IDbCreates<TDb> DbCreates,
        ILoggerCreates<TDb, ELogger> LoggerCreates,
        IValidationHandler<TDb, ELogger, UInput> ValidationHandler,
        ITransformHandler<UInput,TransformOutput> TransformHandler,
        IHandler<TDb, ELogger, TransformOutput, HandlerOutput> Handler,
        IOutputHandler<HandlerOutput, Output> OutputHandler
        ) Destruct() => (DbCreates, LoggerCreates, ValidationHandler, TransformHandler, Handler, OutputHandler);

    public async Task<Transaction<Output>> ToServiceAndRun(UInput input, CancellationToken token = default) {
        return await AttachmentServiceApi<TDb, ELogger, UInput, TransformOutput, HandlerOutput, Output>.RunAsync(this, input, token);
    }
}