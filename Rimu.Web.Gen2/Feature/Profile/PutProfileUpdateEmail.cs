using System.Data;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PutProfileUpdateEmail: FastEndpoints.Endpoint<PutProfileUpdateEmail.PutProfileUpdateEmailRequest,
    Results<Ok, BadRequest<PutProfileUpdateEmail.UpdateEmailBadRequestDto>, InternalServerError, NotFound>
> {
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IDbTransactionContext _dbTransactionContext;

    public PutProfileUpdateEmail(IQueryUserInfo queryUserInfo, IAuthenticationProvider authenticationProvider, IDbTransactionContext dbTransactionContext) {
        _queryUserInfo = queryUserInfo;
        _authenticationProvider = authenticationProvider;
        _dbTransactionContext = dbTransactionContext;
    }

    public override void Configure() {
        Put("/api2/profile/update/email");
        this.AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<PutProfileUpdateEmailRequest>>();
    }

    public override async Task<Results<Ok, BadRequest<UpdateEmailBadRequestDto>, InternalServerError, NotFound>> ExecuteAsync(PutProfileUpdateEmailRequest req, CancellationToken ct) {
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;

        var authContextResult = await _authenticationProvider.GetUserAuthContextByEmail(req.OldEmail);
        if (authContextResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        if (authContextResult.Ok().IsNotSet()) {
            return TypedResults.BadRequest(new UpdateEmailBadRequestDto { OldEmailIsInvalid = true });
        }
        
        var authContext = authContextResult.Ok().Unwrap();
        if (!authContext.FoundAndAuthorized) {
            return TypedResults.BadRequest(new UpdateEmailBadRequestDto { OldEmailIsInvalid = true });
        }

        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Serializable);
        if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        {
            var passwordIsOkResult = await authContext.IsPasswordValidAndSetGen2IfNotExistAsync(req.Password);
            if (passwordIsOkResult == EResult.Err) {
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }

            if (!passwordIsOkResult.Ok()) {
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.BadRequest(new UpdateEmailBadRequestDto { PasswordIsInvalid = true });
            }
        }

        
        var emailExistResult = (await _queryUserInfo.GetByEmailAsync(req.NewEmail)).Map(x => x.IsSet());
        if (emailExistResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        if (emailExistResult.Ok()) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.BadRequest(new UpdateEmailBadRequestDto { NewEmailIsInUse = true });
        }

        if (await _queryUserInfo.UpdateEmailByUserId(userId, req.NewEmail) == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        await _dbTransactionContext.CommitAsync();
        return TypedResults.Ok();
    }


    public sealed class UpdateEmailBadRequestDto {
        public bool PasswordIsInvalid { get; set; }
        public bool OldEmailIsInvalid { get; set; }
        public bool NewEmailIsInUse { get; set; }
    }
    
    public sealed class PutProfileUpdateEmailRequest {
        public string OldEmail { get; set; } = "";
        public string NewEmail { get; set; } = "";
        public string Password { get; set; } = "";
        
        public sealed class PutProfileUpdateEmailRequestValidation: Validator<PutProfileUpdateEmailRequest> {
            public PutProfileUpdateEmailRequestValidation() {
                RuleFor(x => x.OldEmail)
                    .MinimumLength(3)
                    .Must(RegexValidation.ValidateEmail)
                ;
                RuleFor(x => x.NewEmail)
                    .MinimumLength(3)
                    .Must(RegexValidation.ValidateEmail)
                ;
                RuleFor(x => x.Password)
                    .MinimumLength(2)
                    .MaximumLength(128)
                ;
            }
        }
    }
}