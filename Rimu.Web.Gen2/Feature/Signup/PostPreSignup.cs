using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Mailer.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.Share.Signup;

namespace Rimu.Web.Gen2.Feature.Signup;

public sealed class PostPreSignup: Endpoint<
    PostPreSignup.PreSignupRequest, 
    Results<Ok<PostPreSignup.PreSignupResponseDto>, InternalServerError, NotFound>
> {
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IMailerProvider _mailerProvider;
    private readonly IQueryTokenWithGroup _queryTokenWithGroup;
    private readonly IDbTransactionContext _dbTransactionContext;

    public PostPreSignup(IQueryUserInfo queryUserInfo, IMailerProvider mailerProvider, IQueryTokenWithGroup queryTokenWithGroup, IDbTransactionContext dbTransactionContext) {
        _queryUserInfo = queryUserInfo;
        _mailerProvider = mailerProvider;
        _queryTokenWithGroup = queryTokenWithGroup;
        _dbTransactionContext = dbTransactionContext;
    }

    public override void Configure() {
        Post("/api2/signup/pre-signup");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<PreSignupResponseDto>, InternalServerError, NotFound>> ExecuteAsync(PreSignupRequest req, CancellationToken ct) {
        var inUseRequest = await _queryUserInfo.CheckExistByEmailAndUsername(req.Email, req.Username);
        if (inUseRequest == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (inUseRequest.Ok()) {
            return TypedResults.Ok(new PreSignupResponseDto() { EmailOrUsernameInUse = true });
        }
        
        var token = RandomNumberGenerator.GetHexString(10);
        
        var tokenWithGroup = new TokenWithGroup() {
            Data = new TokenWithGroupDataDto() {
                Email = req.Email,
                Username = req.Username,
            }.ToJson(),
            CreateTime = DateTime.UtcNow,
            Group = Rimu.Repository.Postgres.Adapter.Enum.ETokenGroup.SIGNUP_ACCOUNT.ToString(),
            Token = token
        };

        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Serializable);
        if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err
            || await _queryTokenWithGroup.InsertAsync(tokenWithGroup) == EResult.Err
            || _mailerProvider.MainSendSignupEmail(req.Username, req.Email, token) == EResult.Err) {

            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        if (await _dbTransactionContext.CommitAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok(new PreSignupResponseDto() { EmailOrUsernameInUse = false });
    }


    public sealed class PreSignupResponseDto {
        public required bool EmailOrUsernameInUse { get; set; }
    }

    public sealed class PreSignupRequest {
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        
        public sealed class PreSignupRequestValidator : Validator<PreSignupRequest> {
            public PreSignupRequestValidator() {
                RuleFor(static x => x.Email)
                    .Must(RegexValidation.ValidateEmail)
                    ;
                RuleFor(static x => x.Username)
                    .MinimumLength(3)
                    ;
            }
        }
    }
}