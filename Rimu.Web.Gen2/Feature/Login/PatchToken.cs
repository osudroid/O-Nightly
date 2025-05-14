using System.Data;
using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Login;

public class PatchToken: FastEndpoints.EndpointWithoutRequest<
    Results<Ok<TokenTTLDto>, BadRequest<string>, InternalServerError, NotFound<string>, UnauthorizedHttpResult>
> {
    private readonly IApi2TokenProvider _api2TokenProvider;
    private readonly IDbTransactionContext _dbTransaction;
    
    public PatchToken(IApi2TokenProvider api2TokenProvider, IDbTransactionContext dbTransaction) {
        _api2TokenProvider = api2TokenProvider;
        _dbTransaction = dbTransaction;
    }

    public override void Configure() {
        Patch("/api/login/token");
        AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<EmptyRequest>>();
    }
    
    
    

    public override async Task<Results<Ok<TokenTTLDto>, BadRequest<string>, InternalServerError, NotFound<string>, UnauthorizedHttpResult>> ExecuteAsync(CancellationToken ct) {
        var tokenWithTtlDto = ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap();

        if ((await _api2TokenProvider.DeleteTokenInDbAsync(tokenWithTtlDto)) == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        _dbTransaction.SetIsolationLevel(IsolationLevel.Serializable);
        if (await _dbTransaction.BeginTransactionAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        var userTokenResult = await this._api2TokenProvider.UpdateTokenAndUpdateInDbAsync(tokenWithTtlDto);
        if (userTokenResult == EResult.Err) {
            await _dbTransaction.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        tokenWithTtlDto = userTokenResult.Ok();
        
        return TypedResults.Ok(new TokenTTLDto(tokenWithTtlDto.TTLInSeconds));
    }
}