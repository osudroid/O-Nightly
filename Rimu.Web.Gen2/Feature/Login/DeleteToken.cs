using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Login;

public class DeleteToken: FastEndpoints.EndpointWithoutRequest<
    Results<Ok, BadRequest<string>, InternalServerError, NotFound<string>, UnauthorizedHttpResult>
>{
    private readonly IApi2TokenProvider _api2TokenProvider;
    private readonly IQueryUserInfo _queryUserInfo;

    public DeleteToken(IApi2TokenProvider api2TokenProvider, IQueryUserInfo queryUserInfo) {
        _api2TokenProvider = api2TokenProvider;
        _queryUserInfo = queryUserInfo;
        
    }

    public override void Configure() {
        Delete("/api/login/token");
        AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<EmptyRequest>>();
    }

    public override async Task<
        Results<Ok, BadRequest<string>, InternalServerError, NotFound<string>, UnauthorizedHttpResult>> 
        ExecuteAsync(CancellationToken ct) {

        var tokenWithTtlDto = ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap();

        if ((await _api2TokenProvider.DeleteTokenInDbAsync(tokenWithTtlDto)) == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok();
    }
}