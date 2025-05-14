using System.Security.Cryptography;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Mailer.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PostProfileDropAccountSendEmail: FastEndpoints.Endpoint<PostProfileDropAccountSendEmail.ProfileDropAccountSendEmailRequest,
    Results<Ok, BadRequest<PostProfileDropAccountSendEmail.ProfileDropAccountSendEmailBadRequestResponse>, InternalServerError, NotFound>
> {
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IMailerProvider _mailerProvider;
    private readonly IQueryTokenWithGroup _queryTokenWithGroup;

    public PostProfileDropAccountSendEmail(IAuthenticationProvider authenticationProvider, IMailerProvider mailerProvider, IQueryTokenWithGroup queryTokenWithGroup) {
        _authenticationProvider = authenticationProvider;
        _mailerProvider = mailerProvider;
        _queryTokenWithGroup = queryTokenWithGroup;
    }


    public override void Configure() {
        Post("/api2/profile/drop-account/sendMail");
        this.PreProcessor<UserTokenPreProcessor<ProfileDropAccountSendEmailRequest>>();
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest<ProfileDropAccountSendEmailBadRequestResponse>, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileDropAccountSendEmailRequest req, CancellationToken ct) {

        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;

        var userAuthContextResult = await this._authenticationProvider.GetUserAuthContextByUserId(userId);
        if (userAuthContextResult == EResult.Err || userAuthContextResult.Ok().IsNotSet()) {
            return TypedResults.InternalServerError();
        }
        
        var passwordValidResult = await userAuthContextResult.Ok().Unwrap().IsPasswordValidAndSetGen2IfNotExistAsync(req.Password);
        if (passwordValidResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        if (!passwordValidResult.Ok()) {
            return TypedResults.BadRequest(new ProfileDropAccountSendEmailBadRequestResponse { PasswordIsInvalid = true});
        }

        (var email , var username) = userAuthContextResult
                                     .Ok().Unwrap().UserDataContext
                                     .Map(static x => (x.Email, x.Username)).Unwrap()
        ;
        var tokenStr = RandomNumberGenerator.GetHexString(10);

        if (_mailerProvider.MainSendDropAccountVerifyLinkToken(username, email, tokenStr) == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok();
    }

    public sealed class ProfileDropAccountSendEmailBadRequestResponse {
        public required bool PasswordIsInvalid { get; set; }
    }

    public sealed class ProfileDropAccountSendEmailRequest {
        [Microsoft.AspNetCore.Mvc.FromBody]
        public string Password { get; set; } = "";

        public sealed class ProfileDropAccountSendEmailRequestValidator: Validator<ProfileDropAccountSendEmailRequest> {
            public ProfileDropAccountSendEmailRequestValidator() {
                RuleFor(x => x.Password)
                    .MinimumLength(2)
                    .MaximumLength(128)
                    ;
            }
        }
    }
}