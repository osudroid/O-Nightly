using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Handler;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.Post;
using OsuDroid.Validation;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Validation;

namespace OsuDroid.Controllers.Api;

public sealed class Login : ControllerExtensions {
    [HttpPost("/api/weblogin")]
    [PrivilegeRoute("/api/weblogin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    public async Task<IActionResult> WebLogin([FromBody] PostWebLogin prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new WebLoginValidation(),
            new TransformAction<ControllerPostWrapper<PostWebLogin>, ControllerPostWrapper<WebLoginDto>>(
                post => new ControllerPostWrapper<WebLoginDto>(post.Controller, DtoMapper.WebLoginToDto(post.Post))
            ),
            new WebLoginHandler(),
            new ViewExistOrFoundInfoHandler<ViewWebLogin>(),
            new ControllerPostWrapper<PostWebLogin>(ControllerHandlerBuild(), prop)
        );

        return TransactionToIResult(transaction);
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    [HttpPost("/api/webloginwithusername")]
    [PrivilegeRoute("/api/webloginwithusername")]
    public async Task<IActionResult> WebLoginWithUsername([FromBody] PostWebLoginWithUsername prop) {
        
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new WebLoginWithUsernameValidation(),
            new TransformAction<ControllerPostWrapper<PostWebLoginWithUsername>,
                ControllerPostWrapper<WebLoginWithUsernameDto>>(
                post => new ControllerPostWrapper<WebLoginWithUsernameDto>(post.Controller,
                    DtoMapper.WebLoginWithUsernameToDto(post.Post)
                )
            ),
            new WebLoginWithUsernameHandler(),
            new ViewExistOrFoundInfoHandler<ViewWebLogin>(),
            new ControllerPostWrapper<PostWebLoginWithUsername>(ControllerHandlerBuild(), prop)
        );

        return TransactionToIResult(transaction);
    }

    [HttpPost("/api/webregister")]
    [PrivilegeRoute("/api/webregister")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebRegister([FromBody] PostWebRegister prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new WebRegisterValidation(),
            new TransformAction<ControllerPostWrapper<PostWebRegister>, ControllerPostWrapper<WebRegisterDto>>(
                post => new ControllerPostWrapper<WebRegisterDto>(post.Controller,
                    DtoMapper.WebRegisterToDto(post.Post)
                )
            ),
            new WebRegisterHandler(),
            new ViewExistOrFoundInfoHandler<ViewWebLogin>(),
            new ControllerPostWrapper<PostWebRegister>(ControllerHandlerBuild(), prop)
        );

        return TransactionToIResult(transaction);
    }

    [HttpGet("/api/weblogintoken")]
    [PrivilegeRoute("/api/weblogintoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLoginToken>))]
    public async Task<IActionResult> WebLoginToken() {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
            new TransformParse<ControllerWrapper>(),
            new WebLoginTokenHandler(),
            new ViewExistOrFoundInfoHandler<ViewWebLoginToken>(),
            new ControllerWrapper(ControllerHandlerBuild())
        );

        return TransactionToIResult(transaction);
    }

    [HttpGet("/api/webupdateCookie")]
    [PrivilegeRoute("/api/webupdateCookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>))]
    public async Task<IActionResult> WebUpdateCookie() {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
            new TransformParse<ControllerWrapper>(),
            new WebUpdateCookieHandler(),
            new ViewExistOrFoundInfoHandler<ViewUpdateCookieInfo>(),
            new ControllerWrapper(ControllerHandlerBuild())
        );

        return TransactionToIResult(transaction);
    }


    [HttpPost("/api/webresetpasswdandsendemail")]
    [PrivilegeRoute("/api/webresetpasswdandsendemail")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewResetPasswdAndSendEmail>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswdAndSendEmail([FromBody] PostResetPasswdAndSendEmail prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ResetPasswdAndSendEmailValidation(),
            new TransformAction<ControllerPostWrapper<PostResetPasswdAndSendEmail>,
                ControllerPostWrapper<ResetPasswdAndSendEmailDto>>(
                i => new ControllerPostWrapper<ResetPasswdAndSendEmailDto>(i.Controller,
                    DtoMapper.ResetPasswdAndSendEmailToDto(i.Post)
                )
            ),
            new ResetPasswdAndSendEmailHandler(),
            new ViewExistOrFoundInfoHandler<ViewResetPasswdAndSendEmail>(),
            new ControllerPostWrapper<PostResetPasswdAndSendEmail>(ControllerHandlerBuild(), prop)
        );

        return TransactionToIResult(transaction);
    }

    [HttpPost("/api/token/newpasswdwithtoken")]
    [PrivilegeRoute("/api/token/newpasswdwithtoken")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebReplacePasswordWithToken>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetNewPasswd([FromBody] PostApi.PostApi2GroundNoHeader<PostSetNewPasswd> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new SetNewPasswdValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSetNewPasswd>>,
                ControllerPostWrapper<SetNewPasswdDto>>(i =>
                new ControllerPostWrapper<SetNewPasswdDto>(i.Controller, DtoMapper.SetNewPasswdToDto(i.Post.Body!))
            ),
            new SetNewPasswdHandler(),
            new ViewExistOrFoundInfoHandler<ViewWebReplacePasswordWithToken>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSetNewPasswd>>(ControllerHandlerBuild(), prop)
        );

        return TransactionToIResult(transaction);
    }

    [HttpPost("/api/signin/patreon")]
    [PrivilegeRoute("/api/signin/patreon")]
    public async Task<IActionResult> PatreonSignLogin([FromForm] string provider) {
        await using var dbN = await DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        var isComplete = false;

        try {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider)) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return BadRequest("provider IsNullOrWhiteSpace");
            }

            if (await HttpContext.IsProviderSupportedAsync(provider)) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return BadRequest();
            }


            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);
        }
        catch (Exception e) {
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }

            return InternalServerError();
        }
        finally {
            if (!isComplete) await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/signout/patreon")]
    [PrivilegeRoute("/api/signout/patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatreonSignout() {
        await using var dbN = await DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        var isComplete = false;

        try {
            // TODO PatreonSignout


            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return Ok();
        }
        catch (Exception e) {
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }

            return InternalServerError();
        }
        finally {
            if (!isComplete) await dbT.CommitAsync();
        }
    }


    [HttpGet("/api/weblogout")]
    [PrivilegeRoute("/api/weblogout")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveCookie() {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
            new TransformParse<ControllerWrapper>(),
            new RemoveCookieHandler(),
            new ViewExistOrFoundInfoHandler<ApiTypes.ViewWork>(),
            new ControllerWrapper(ControllerHandlerBuild())
        );

        return TransactionToIResult(transaction);
    }
}