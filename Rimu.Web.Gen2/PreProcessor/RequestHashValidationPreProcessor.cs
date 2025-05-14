using FastEndpoints;
using FluentValidation.Results;
using LamLibAllOver.ErrorHandling;
using NLog;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Security.Adapter.Interface;
using Rimu.Web.Gen2.Interface;

namespace Rimu.Web.Gen2.PreProcessor;

public sealed class RequestHashValidationPreProcessor<TBody, EData>: 
    PreProcessor<TBody, RequestHashValidationPreProcessorState> 
    where EData : class, ISingleString, new()
    where TBody : class, IHashData<EData>, new() {
    
    private readonly ISecurityProvider _securityProvider;
    private readonly IEnvDb _envDb;

    public RequestHashValidationPreProcessor(ISecurityProvider securityProvider, IEnvDb envDb) {
        _securityProvider = securityProvider;
        _envDb = envDb;
    }

    public override Task PreProcessAsync(IPreProcessorContext<TBody> context, RequestHashValidationPreProcessorState state, CancellationToken ct) {
        state.HashIsValid = false;

        ValidationFailure validationFailure = new (
            "RequestHashValidationPreProcessor", 
            "Invalid request hash."
        );
        
        var request = context.Request;
        
        if (request is null or { Data: null } or { Hash: null }) {
            context.ValidationFailures.Add(validationFailure);
            return Task.CompletedTask;
        }

        state.HashIsValid =
            _securityProvider.Security.Api2HashValidate(request.Hash, request.Data.ToSingleString(), _envDb.RequestHash_Keyword);

        if (!state.HashIsValid) {
            context.ValidationFailures.Add(validationFailure);
        }

        return Task.CompletedTask;
    }
}