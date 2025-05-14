using System.Data;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Login;

public class PostLoginEmailPassword: FastEndpoints.Endpoint<
    PostLoginEmailPassword.PostLoginByEmailRequest, 
    Results<Ok<TokenTTLDto>, BadRequest<string>, InternalServerError, NotFound<string>, UnauthorizedHttpResult>
> {
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IApi2TokenProvider _api2TokenProvider;
    private readonly IDbTransactionContext _dbTransaction;
    
    public PostLoginEmailPassword(
        IAuthenticationProvider authenticationProvider, 
        IApi2TokenProvider api2TokenProvider, 
        IDbTransactionContext dbTransaction) {
        
        _authenticationProvider = authenticationProvider;
        _api2TokenProvider = api2TokenProvider;
        _dbTransaction = dbTransaction;
    }

    public override void Configure() {
        Post("/api/login/by/email");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<TokenTTLDto>, BadRequest<string>, InternalServerError, NotFound<string>, UnauthorizedHttpResult>> ExecuteAsync(PostLoginByEmailRequest req, CancellationToken ct) {
        var email = req.Email;
        var password = req.Password;

        
        var authContextResult = await _authenticationProvider.GetUserAuthContextByEmail(email);
        if (authContextResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (authContextResult.Ok().IsNotSet()) {
            return TypedResults.NotFound("Email not found");
        }

        var authContext = authContextResult.Ok().Unwrap();
        if (!authContext.FoundAndAuthorized) {
            return TypedResults.Unauthorized();
        }

        var validPasswordResult = await authContext.IsPasswordValidAndSetGen2IfNotExistAsync(password);
        if (validPasswordResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (!validPasswordResult.Ok()) {
            return TypedResults.Unauthorized();
        }

        
        _dbTransaction.SetIsolationLevel(IsolationLevel.Serializable);
        if (await _dbTransaction.BeginTransactionAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        var userTokenResult = await this._api2TokenProvider.CreateTokenAndInsertAsync(authContext.UserDataContext.Unwrap().UserId);
        if (userTokenResult == EResult.Err) {
            await _dbTransaction.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        var userToken = userTokenResult.Ok();
        
        
        await this.SendHeadersAsync(dictionary => {
            dictionary.Authorization = new StringValues(userToken.Token);
            dictionary.ContentType = "application/json";
        }, 200, ct);

        
        return TypedResults.Ok(new TokenTTLDto(userToken.TTLInSeconds));
    }

    public class PostLoginByEmailRequest {
        private string _email = "";
        private string _password = "";

        public string Email {
            get => _email;
            set => _email = value.Trim();
        }

        public string Password {
            get => _password;
            set => _password = value.Trim();
        }
    }

    public class PostLoginByEmailRequestValidator: Validator<PostLoginByEmailRequest> {
        public PostLoginByEmailRequestValidator() {
            RuleFor(x => x.Email)
                .Must(RegexValidation.ValidateEmail)
            ;
            RuleFor(x => x.Password)
                .MinimumLength(2)
                .MaximumLength(128);
        }
    }
}