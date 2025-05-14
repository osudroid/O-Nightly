using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Profile;

public sealed class PutProfileUpdateAvatar: FastEndpoints.Endpoint<PutProfileUpdateAvatar.ProfileUpdateAvatarRequest,
    Results<Ok, BadRequest, InternalServerError, NotFound>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IUserAvatarProvider _userAvatarProvider;

    public PutProfileUpdateAvatar(IUserAvatarProvider userAvatarProvider) {
        _userAvatarProvider = userAvatarProvider;
    }

    public override void Configure() {
        Put("/api2/profile/update/avatar");
        this.AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<PutProfileUpdateAvatar.ProfileUpdateAvatarRequest>>();
    }

    public override async Task<Results<Ok, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(ProfileUpdateAvatarRequest req, CancellationToken ct) {
        
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;
        
        var imageBytes = Array.Empty<byte>();
        try {
            var charArr = req.Base64.AsSpan(req.Base64.IndexOf(',') + 1).ToArray();
            // TODO Write one Convert.FromBase64String With Span
            imageBytes = Convert.FromBase64CharArray(charArr, 0, charArr.Length);
        }
        catch (Exception e) {
            Logger.Error(e);
            return TypedResults.InternalServerError();
        }
        
        var avatarContext = _userAvatarProvider.CreateNewContext(userId);
        return (await avatarContext.InsertOrOverwriteWithNewAvatarAsync(imageBytes))
            .Map<Results<Ok, BadRequest, InternalServerError, NotFound>>(static _ => TypedResults.Ok())
            .OkOr(TypedResults.InternalServerError());
    }

    public sealed class ProfileUpdateAvatarRequest {
        [Microsoft.AspNetCore.Mvc.FromBody] public string Base64 { get; set; } = "";
        
        public sealed class ProfileUpdateAvatarRequestValidation: Validator<ProfileUpdateAvatarRequest> {
            public ProfileUpdateAvatarRequestValidation() {
                RuleFor(x => x.Base64)
                    .MinimumLength(4)
                    .MaximumLength(10_000_000)
                    .Must(RegexValidation.ValidateBase64)
                ;
            }
        }
    }
}