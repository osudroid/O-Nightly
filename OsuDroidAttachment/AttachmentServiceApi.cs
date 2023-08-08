using System.Transactions;
using LamLibAllOver;
using OsuDroidAttachment.Interface;
using OsuDroidAttachment.Class;

namespace OsuDroidAttachment; 

public sealed class AttachmentServiceApi<TDb, ELogger, UInput, TransformOutput, HandlerOutput, Output>
    where TDb: IDb 
    where ELogger: OsuDroidAttachment.Interface.ILogger
    where UInput: IInput 
    where TransformOutput: ITransformOutput
    where HandlerOutput: IHandlerOutput {

    public static async Task<Transaction<Output>> RunAsync(
        AttachmentServiceApiBuilder<TDb, ELogger, UInput, TransformOutput, HandlerOutput, Output> builder,
        UInput input,
        CancellationToken token = default) {
        
        try
        {
            var result = Task.Factory.StartNew(() => RunFullAsync(builder, input), TaskCreationOptions.PreferFairness).Unwrap();
            await result.WaitAsync(token);
            if (result.IsCompleted) {
                return await builder.OutputHandler.Handel(Result<HandlerOutput, string>.Err("IsNotCompleted"));
            }

            return result.Result;
        }
        catch (Exception e)
        {
            return await builder.OutputHandler.Handel(Result<HandlerOutput, string>.Err(e.ToString()));
        }
    }
    
    private static async Task<Transaction<Output>> RunFullAsync(
        AttachmentServiceApiBuilder<TDb, ELogger, UInput, TransformOutput, HandlerOutput, Output> builder,
        UInput input) {
        
        var (dbCreates, loggerCreates, validationHandler, transformHandler, handler, outputHandler) = builder.Destruct();

        var dbResult = await dbCreates.Create();
        if (dbResult == EResult.Err) {
            return await outputHandler.Handel(dbResult.ChangeOkType<HandlerOutput>());
        }
        await using TDb db = dbResult.Ok();
        
        await using ELogger logger = await loggerCreates.Create(db);

        try {


            var validateResult = await validationHandler.Validate(db, logger, input);
            if (validateResult == EResult.Err) {
                await db.RollbackAsync();
                return await outputHandler.Handel(validateResult.ChangeOkType<HandlerOutput>());
            }

            var validateHashResult = await validationHandler.HashMatch(logger, input);
            if (validateHashResult == EResult.Err) {
                await db.RollbackAsync();
                return await outputHandler.Handel(validateResult.ChangeOkType<HandlerOutput>());
            }

            if (!validateResult.Ok()) {
                await db.RollbackAsync();
                return Transaction<Output>.BadRequest();
            }

            var transformOutputResult = await transformHandler.Transform(input);
            if (transformOutputResult == EResult.Err) {
                await db.RollbackAsync();
                return await outputHandler.Handel(transformOutputResult.ChangeOkType<HandlerOutput>());
            }

            var transformOutput = transformOutputResult.Ok();

            var handelResult = await handler.Handel(db, logger, transformOutput);
            return await outputHandler.Handel(handelResult);
        }

        catch (Exception e) {
            await db.RollbackAsync();
            return await builder.OutputHandler.Handel(Result<HandlerOutput, string>.Err(e.ToString()));
        }
        finally {
            try {
                await db.CommitAsync();
            }
            catch {
                // ignored
            }
        }
    }
}