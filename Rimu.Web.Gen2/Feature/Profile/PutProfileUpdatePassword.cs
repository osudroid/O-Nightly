using System.Data;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PutProfileUpdatePassword: FastEndpoints.Endpoint<PutProfileUpdatePassword.PutProfileUpdatePasswordRequest,
    Results<Ok, BadRequest<PutProfileUpdatePassword.PutProfileUpdatePasswordResponseDto>, InternalServerError, NotFound>
> {
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IDbTransactionContext _dbTransactionContext;

    public PutProfileUpdatePassword(IQueryUserInfo queryUserInfo, IAuthenticationProvider authenticationProvider, IDbTransactionContext dbTransactionContext) {
        _queryUserInfo = queryUserInfo;
        _authenticationProvider = authenticationProvider;
        _dbTransactionContext = dbTransactionContext;
    }

    public override void Configure() {
        Put("/api2/profile/update/password");
        this.AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<PutProfileUpdatePasswordRequest>>();
    }

    public override async Task<Results<Ok, BadRequest<PutProfileUpdatePasswordResponseDto>, InternalServerError, NotFound>> 
        ExecuteAsync(PutProfileUpdatePasswordRequest req, CancellationToken ct) {
     
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;
        
        var authContextResult = await _authenticationProvider.GetUserAuthContextByUserId(userId);
        if (authContextResult == EResult.Err || authContextResult.Ok().IsNotSet()) {
            return TypedResults.InternalServerError();
        }
        
        var authContext = authContextResult.Ok().Unwrap();
        if (!authContext.FoundAndAuthorized) {
            return TypedResults.InternalServerError();
        }

        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Serializable);
        if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        {
            var passwordIsOkResult = authContext.IsPassword(req.OldPassword);
            if (passwordIsOkResult == EResult.Err) {
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }

            if (!passwordIsOkResult.Ok()) {
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.BadRequest(new PutProfileUpdatePasswordResponseDto { OldPasswordIsInValid = true });
            }

            var changePasswordResult = await authContext.UpdatePasswordAsync(req.NewPassword);
            if (changePasswordResult == EResult.Err) {
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }
        }

        await _dbTransactionContext.CommitAsync();
        
        return TypedResults.Ok();
    }

    public class PutProfileUpdatePasswordResponseDto {
        public bool OldPasswordIsInValid { get; set; }
    }

    public sealed class PutProfileUpdatePasswordRequest {
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
        
        public sealed class PutProfileUpdatePasswordRequestValidation: Validator<PutProfileUpdatePasswordRequest> {
            public PutProfileUpdatePasswordRequestValidation() {
                RuleFor(x => x.OldPassword)
                    .MinimumLength(2)
                    .MaximumLength(128)
                    ;
                RuleFor(x => x.NewPassword)
                    .MinimumLength(2)
                    .MaximumLength(128)
                    ;
            }
        }
    }
}