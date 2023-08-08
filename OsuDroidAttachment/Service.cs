using Microsoft.Extensions.Logging;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment; 

public static class Service {
    public static async Task<Transaction<Output>> AttachmentServiceApi<TDb, ELogger, UInput, TransformOutput, HandlerOutput, Output>(
        IDbCreates<TDb> dbCreates,
        ILoggerCreates<TDb, ELogger> loggerCreates,
        IValidationHandler<TDb, ELogger, UInput> validationHandler,
        ITransformHandler<UInput,TransformOutput> transformHandler,
        IHandler<TDb, ELogger, TransformOutput, HandlerOutput> handler,
        IOutputHandler<HandlerOutput, Output> outputHandler,
        UInput input,
        CancellationToken token = default)
        where TDb: IDb 
        where ELogger: OsuDroidAttachment.Interface.ILogger
        where UInput: IInput 
        where TransformOutput: ITransformOutput
        where HandlerOutput: IHandlerOutput {

        return await new OsuDroidAttachment.AttachmentServiceApiBuilder<TDb, ELogger, UInput, TransformOutput, HandlerOutput,
            Output>() {
            DbCreates = dbCreates,
            LoggerCreates = loggerCreates,
            ValidationHandler = validationHandler,
            TransformHandler = transformHandler,
            Handler = handler,
            OutputHandler = outputHandler,
        }.ToServiceAndRun(input, token);
    }
}