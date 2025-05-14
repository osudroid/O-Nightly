using LamLibAllOver.ErrorHandling;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Attachment.Adapter.Class;
using Rimu.Repository.Attachment.Adapter.Enum;
using Rimu.Repository.Attachment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Attachment.Domain;

public class AttachmentServiceApi<HandlerInput, HandlerOutput> : IAttachmentServiceApi<HandlerInput, HandlerOutput> 
    where HandlerInput: MediatR.IRequest<bool>, IRequest<SResult<OptionHandlerOutput<HandlerOutput>>> {
    
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger(); 
    private readonly IDbTransactionContext _transactionContext;
    private readonly AsyncServiceScope _asyncServiceScope;
    private readonly IServiceProvider _serviceScope;
    private readonly ScopeGuid _scopeGuid;
    private readonly Mediator _mediator;
    private readonly HandlerInput _handlerInput;
    private volatile bool _isUsed;

    private AttachmentServiceApi(
        IDbTransactionContext transactionContext, 
        HandlerInput handlerInput,
        Mediator mediator,
        ScopeGuid scopeGuid,
        AsyncServiceScope asyncServiceScope) {
        
        _transactionContext = transactionContext;
        _asyncServiceScope = asyncServiceScope;
        _handlerInput = handlerInput;
        _mediator = mediator;
        _scopeGuid = scopeGuid;
        _serviceScope = _asyncServiceScope.ServiceProvider;
        
        _isUsed = false;
    }

    public static async Task<AttachmentServiceApi<HandlerInput, HandlerOutput>> CreateAsync(
        HandlerInput handlerInput, 
        AsyncServiceScope asyncServiceScope) {

        var serviceProvider = asyncServiceScope.ServiceProvider;
        var mediator = serviceProvider.GetService<Mediator>() ?? throw new InvalidOperationException();
        var scopeGuid = serviceProvider.GetService<ScopeGuid>() ?? throw new InvalidOperationException();
        var databaseContext = serviceProvider.GetService<DatabaseContext>() ?? throw new InvalidOperationException();;

        await databaseContext.GetDbAsync();
        return new AttachmentServiceApi<HandlerInput, HandlerOutput>(
            databaseContext, 
            handlerInput,
            mediator, 
            scopeGuid,
            asyncServiceScope
        );
    }
    
    public void Dispose() {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async Task<Transaction<HandlerOutput>> RunOperationAsync(HandlerInput handlerInput, CancellationToken token) {
        lock (this) {
            if (this._isUsed) {
                throw new Exception("Operation can Only Run 1");
            }

            this._isUsed = true;
        }

        try {
            var err = await this._transactionContext.StartTransactionAsync(IsolationLevel.Serializable);
            if (err == EResult.Err) {
                Logger.Error(err.Err().ToString);
                return Transaction<HandlerOutput>.InternalServerError(err);
            }

            var db = await this._transactionContext.GetDbAsync();

            if (!await _mediator.Send<bool>(handlerInput, token)) {
                Logger.Debug("Input Not Valid");
                return Transaction<HandlerOutput>.BadRequest();
            }

            var handlerResult = await _mediator.Send<SResult<OptionHandlerOutput<HandlerOutput>>>(handlerInput);
            
            if (handlerResult == EResult.Err) {
                Logger.Error(err.Err().ToString);
                return Transaction<HandlerOutput>.InternalServerError(handlerResult);
            }

            return new Transaction<HandlerOutput>(EModelResult.Ok, handlerResult.Ok().Option, SResultErr.Ok());
        }
        catch (Exception e) {
            Logger.Error(e);
            try
            {
                await this._transactionContext.RollbackTransactionAsync();
            }
            catch (Exception) {
                // ignored
            }
            
            return Transaction<HandlerOutput>.InternalServerError(SResultErr.Err(e));
        }
        finally {
            try {
                await this._transactionContext.CommitTransactionAsync();
            }
            catch {
                // ignored
            }
        }
    }
    
    
    // private static async Task<Transaction<Output>> RunAsync(
    //     AttachmentServiceApiBuilder<UInput, TransformOutput, HandlerOutput, Output> builder,
    //     UInput input,
    //     CancellationToken token = default) {
    //     try {
    //         var stopwatch = Stopwatch.StartNew();
    //         var result = Task.Factory.StartNew(() => RunFullAsync(builder, input), TaskCreationOptions.PreferFairness)
    //                          .Unwrap();
    //         await result.WaitAsync(token);
    //         if (result.IsCompleted == false) {
    //             var resultValue = await builder.OutputHandler.Handel(
    //                 SResult<HandlerOutput>.Err(TraceMsg.WithMessage("IsNotCompleted"))
    //             );
    //
    //             stopwatch.Stop();
    //             Console.WriteLine(
    //                 $"Time need {stopwatch.ElapsedMilliseconds}ms; Handler: {builder.Handler.GetType()}; Status IsNotCompleted"
    //             );
    //             return resultValue;
    //         }
    //
    //         stopwatch.Stop();
    //         Console.WriteLine($"Time need {stopwatch.ElapsedMilliseconds}ms; Handler: {builder.Handler.GetType()}");
    //         return result.Result;
    //     }
    //     catch (Exception e) {
    //         return await builder.OutputHandler.Handel(SResult<HandlerOutput>.Err(e.ToString()));
    //     }
    // }
    //
    // private static async Task<Transaction<Output>> RunFullAsync(
    //     AttachmentServiceApiBuilder<UInput, TransformOutput, HandlerOutput, Output> builder,
    //     UInput input) {
    //     
    //     (ITransformHandler<UInput, TransformOutput> transformHandler, Handler<TransformOutput, HandlerOutput> handler, IOutputHandler<HandlerOutput, Output> outputHandler) = builder.Destruct();
    //     
    //     await using var asyncScope = Collection.Provider.CollectionHolder.GlobalServiceProvider.CreateAsyncScope();
    //     var serviceProvider = asyncScope.ServiceProvider;
    //     DatabaseContext databaseContext = serviceProvider.GetService<DatabaseContext>() ?? throw new InvalidOperationException();
    //     var mediator = serviceProvider.GetService<Mediator>() ?? throw new InvalidOperationException();
    //     await databaseContext.StartTransactionAsync(IsolationLevel.Serializable);
    //     var db = await databaseContext.GetDbAsync();
    //     
    //     var transaction = await db.BeginTransactionAsync();
    //     try {
    //         var validateResult = await mediator.Send(input);    
    //         if (validateResult == EResult.Err) {
    //             await transaction.RollbackAsync();
    //             return await outputHandler.Handel(validateResult.ChangeOkType<HandlerOutput>());
    //         }
    //
    //         if (!validateResult.Ok()) {
    //             await transaction.RollbackAsync();
    //             return Transaction<Output>.BadRequest();
    //         }
    //
    //         var transformOutputResult = await transformHandler.Transform(input);
    //         if (transformOutputResult == EResult.Err) {
    //             await transaction.RollbackAsync();
    //             return await outputHandler.Handel(transformOutputResult.ChangeOkType<HandlerOutput>());
    //         }
    //
    //         var transformOutput = transformOutputResult.Ok();
    //         
    //         
    //         var handelResult = await handler.Handel(db, transformOutput);
    //         
    //         return await outputHandler.Handel(handelResult);
    //     }
    //
    //     catch (Exception e) {
    //         await transaction.RollbackAsync();
    //         return await builder.OutputHandler.Handel(SResult<HandlerOutput>.Err(e));
    //     }
    //     finally {
    //         try {
    //             await transaction.CommitAsync();
    //         }
    //         catch {
    //             // ignored
    //         }
    //     }
    // }

    public async ValueTask DisposeAsync() {
        GC.SuppressFinalize(this);
        await _transactionContext.DisposeAsync();
        await _asyncServiceScope.DisposeAsync();
    }
}