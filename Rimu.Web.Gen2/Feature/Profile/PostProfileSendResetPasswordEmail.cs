using System.Data;
using System.Security.Cryptography;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Mailer.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PostProfileSendResetPasswordEmail: Endpoint<
    PostProfileSendResetPasswordEmail.ProfileSendResetPasswordEmailRequest, 
    Results<Ok, InternalServerError, NotFound>
> {
    private readonly IMailerProvider _mailerProvider;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly IQueryTokenWithGroup _queryTokenWithGroup;

    public PostProfileSendResetPasswordEmail(IMailerProvider mailerProvider, IQueryUserInfo queryUserInfo, IAuthenticationProvider authenticationProvider, IDbTransactionContext dbTransactionContext, IQueryTokenWithGroup queryTokenWithGroup) {
        _mailerProvider = mailerProvider;
        _queryUserInfo = queryUserInfo;
        _authenticationProvider = authenticationProvider;
        _dbTransactionContext = dbTransactionContext;
        _queryTokenWithGroup = queryTokenWithGroup;
    }

    public override void Configure() {
        Post("/api2/profile/send-reset-password-email");
        AllowAnonymous();
    }

    public override async Task<Results<Ok, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileSendResetPasswordEmailRequest req, CancellationToken ct) {

        var userAuthContextResult = await _authenticationProvider.GetUserAuthContextByEmail(req.Email);

        if (userAuthContextResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (userAuthContextResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }

        var userAuthContext = userAuthContextResult.Ok().Unwrap();
        var userAuthDataContext = userAuthContext.UserDataContext.Unwrap();
        if (!userAuthContext.FoundAndAuthorized) {
            return TypedResults.NotFound();
        }

        
        {
            _dbTransactionContext.SetIsolationLevel(IsolationLevel.Serializable);
            if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err) {
                return TypedResults.InternalServerError();
            }
            
            var token = RandomNumberGenerator.GetHexString(20);
            
            if (await _queryTokenWithGroup.InsertAsync(new TokenWithGroup() {
                        Data = userAuthDataContext.UserId.ToString(),
                        CreateTime = DateTime.UtcNow,
                        Group = ETokenGroup.RESET_PASSWORD.ToString(),
                        Token = token
                    }
                )
                == EResult.Err) {
                
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }
            
            if (_mailerProvider
                    .MainSendResetEmail(userAuthDataContext.UserId, userAuthDataContext.Username, req.Email, token) 
                == EResult.Err) {

                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }
            
            if (await _dbTransactionContext.CommitAsync() == EResult.Err) {
                return TypedResults.InternalServerError();
            }
        }
        
        return TypedResults.Ok();
    }


    public sealed class ProfileSendResetPasswordEmailRequest {
        [FromBody]
        public string Email { get; set; } = "";

        public sealed class ProfileSendResetPasswordEmailRequestValidator : Validator<ProfileSendResetPasswordEmailRequest> {
            public ProfileSendResetPasswordEmailRequestValidator() {
                RuleFor(static x => x.Email)
                    .Must(RegexValidation.ValidateEmail)
                    ;
            }
        }
    }
}