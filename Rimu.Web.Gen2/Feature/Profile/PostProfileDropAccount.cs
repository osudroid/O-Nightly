using System.Data;
using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PostProfileDropAccount: FastEndpoints.Endpoint<PostProfileDropAccount.ProfileDropAccountRequest,
    Results<Ok, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryTokenWithGroup _queryTokenWithGroup;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly IEnvDb _envDb;

    public PostProfileDropAccount(IQueryUserInfo queryUserInfo, IQueryTokenWithGroup queryTokenWithGroup, IDbTransactionContext dbTransactionContext, IEnvDb envDb) {
        _queryUserInfo = queryUserInfo;
        _queryTokenWithGroup = queryTokenWithGroup;
        _dbTransactionContext = dbTransactionContext;
        _envDb = envDb;
    }

    public override void Configure() {
        Post("/api2/profile/drop-account");
        this.PreProcessor<UserTokenPreProcessor<ProfileDropAccountRequest>>();
    }

    public async override Task<Results<Ok, BadRequest, InternalServerError, NotFound>> ExecuteAsync(ProfileDropAccountRequest req, CancellationToken ct) {
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;

        
        var findTokenResult = await _queryTokenWithGroup.FindByTokenGroupAndTokenAsync(ETokenGroup.DropAccount, req.Token);
        if (findTokenResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (findTokenResult.Ok().IsNotSet()
            || findTokenResult.Ok().Unwrap().CreateTime.Add(_envDb.TokenDropAccount_TTL) < DateTime.UtcNow) {
            return TypedResults.BadRequest();
        }


        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Snapshot);
        if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        
        if (await _queryUserInfo.SetActiveAsync(userId, false) == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        if (await _queryTokenWithGroup.DeleteAsync(ETokenGroup.DropAccount, req.Token) == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        if (await _dbTransactionContext.CommitAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok();
    }

    public sealed class ProfileDropAccountRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "token")] public string Token { get; set; } = "";
    }
}