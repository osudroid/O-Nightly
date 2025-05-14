using System.Data;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Mailer.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PutProfileResetPassword: Endpoint<
    PutProfileResetPassword.ProfileResetPasswordRequest, 
    Results<Ok, InternalServerError, NotFound, Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>
> {
    private readonly IMailerProvider _mailerProvider;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly IQueryTokenWithGroup _queryTokenWithGroup;
    private readonly IEnvDb _envDb;

    public PutProfileResetPassword(IMailerProvider mailerProvider, IQueryUserInfo queryUserInfo, IAuthenticationProvider authenticationProvider, IDbTransactionContext dbTransactionContext, IQueryTokenWithGroup queryTokenWithGroup, IEnvDb envDb) {
        _mailerProvider = mailerProvider;
        _queryUserInfo = queryUserInfo;
        _authenticationProvider = authenticationProvider;
        _dbTransactionContext = dbTransactionContext;
        _queryTokenWithGroup = queryTokenWithGroup;
        _envDb = envDb;
    }

    public override void Configure() {
        Put("/api2/profile/reset-password");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok, InternalServerError, NotFound, UnauthorizedHttpResult>> 
        ExecuteAsync(ProfileResetPasswordRequest req, CancellationToken ct) {

        
        
        var tokenGroupAndTokenResult = await _queryTokenWithGroup.FindByTokenGroupAndTokenAsync(ETokenGroup.RESET_PASSWORD, req.Token);
        if (tokenGroupAndTokenResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (tokenGroupAndTokenResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }

        TokenWithGroup tokenGroupAndToken = tokenGroupAndTokenResult.Ok().Unwrap();
        if (tokenGroupAndToken.CreateTime.Add(_envDb.TokenResetPassword_TTL) < DateTime.UtcNow) {
            return TypedResults.Unauthorized();
        }

        if (!long.TryParse(tokenGroupAndToken.Data, out var userId)) {
            return TypedResults.InternalServerError();
        }
        
        var authContextResult = await _authenticationProvider.GetUserAuthContextByUserId(userId);
        if (authContextResult == EResult.Err || authContextResult.Ok().IsNotSet()) {
            return TypedResults.InternalServerError();
        }

        var authContext = authContextResult.Ok().Unwrap();

        if (!authContext.FoundAndAuthorized) {
            return TypedResults.Unauthorized();
        }

        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Serializable);
        
        if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err 
            || await authContext.UpdatePasswordAsync(req.NewPassword) == EResult.Err
            || await _queryTokenWithGroup.DeleteAsync(ETokenGroup.RESET_PASSWORD, req.Token) == EResult.Err
            || await _dbTransactionContext.CommitAsync() == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok();
    }

    public sealed class ProfileResetPasswordRequest {
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";

        public sealed class ProfileResetPasswordRequestValidator : Validator<ProfileResetPasswordRequest> {
            public ProfileResetPasswordRequestValidator() {
                RuleFor(x => x.Token)
                    .MinimumLength(1);
                RuleFor(x => x.NewPassword)
                    .MinimumLength(2)
                    .MaximumLength(128)
                    ;
            }
            
        }
    }
}