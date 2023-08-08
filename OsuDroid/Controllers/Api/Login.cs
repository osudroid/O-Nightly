using System.Data;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Post;
using OsuDroid.View;
using OsuDroid.Model;
using OsuDroid.OutputHandler;
using OsuDroid.Validation;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Validation;
using EModelResult = OsuDroid.Class.EModelResult;

namespace OsuDroid.Controllers.Api;

#nullable enable

public sealed class Login : ControllerExtensions {
    [HttpPost("/api/weblogin")]
    [PrivilegeRoute(route: "/api/weblogin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    public async Task<IActionResult> WebLogin([FromBody] PostWebLogin prop) {
        Transaction<ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>> transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
            NpgsqlCreates.DbWrapper, 
            LogWrapper, 
            ControllerPostWrapper<PostWebLogin>, 
            ControllerPostWrapper<WebLoginDto>, 
            OptionHandlerOutput<ViewWebLogin>, 
            ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>
            >(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new WebLoginValidation(),
            transformHandler: new TransformAction<ControllerPostWrapper<PostWebLogin>, ControllerPostWrapper<WebLoginDto>>(
                (post) => new ControllerPostWrapper<WebLoginDto>(post.Controller, DtoMapper.WebLoginToDto(post.Post))),
            handler: new Handler.WebLoginHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewWebLogin>(),
            input: new ControllerPostWrapper<PostWebLogin>(this.ControllerHandlerBuild(), prop),
            default
            );
        
        return TransactionToIResult(transaction);
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    [HttpPost("/api/webloginwithusername")]
    [PrivilegeRoute(route: "/api/webloginwithusername")]
    public async Task<IActionResult> WebLoginWithUsername([FromBody] PostWebLoginWithUsername prop) {
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
            NpgsqlCreates.DbWrapper, 
            LogWrapper, 
            ControllerPostWrapper<PostWebLoginWithUsername>, 
            ControllerPostWrapper<WebLoginWithUsernameDto>, 
            OptionHandlerOutput<ViewWebLogin>, 
            ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>
        >(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new WebLoginWithUsernameValidation(),
            transformHandler: new TransformAction<ControllerPostWrapper<PostWebLoginWithUsername>, ControllerPostWrapper<WebLoginWithUsernameDto>>(
                (post) => new ControllerPostWrapper<WebLoginWithUsernameDto>(post.Controller, DtoMapper.WebLoginWithUsernameToDto(post.Post))),
            handler: new Handler.WebLoginWithUsernameHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewWebLogin>(),
            input: new ControllerPostWrapper<PostWebLoginWithUsername>(this.ControllerHandlerBuild(), prop),
            default
        );
        
        return TransactionToIResult(transaction);
        
 
    }

    [HttpPost("/api/webregister")]
    [PrivilegeRoute(route: "/api/webregister")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLogin>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebRegister([FromBody] PostWebRegister prop) {
        
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new WebRegisterValidation(),
            transformHandler: new TransformAction<ControllerPostWrapper<PostWebRegister>, ControllerPostWrapper<WebRegisterDto>>(
                (post) => new ControllerPostWrapper<WebRegisterDto>(post.Controller, DtoMapper.WebRegisterToDto(post.Post))),
            handler: new Handler.WebRegisterHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewWebLogin>(),
            input: new ControllerPostWrapper<PostWebRegister>(this.ControllerHandlerBuild(), prop),
            default
        );
        
        return TransactionToIResult(transaction);
    }

    [HttpGet("/api/weblogintoken")]
    [PrivilegeRoute(route: "/api/weblogintoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebLoginToken>))]
    public async Task<IActionResult> WebLoginToken() {
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new OsuDroidAttachment.Validation.ValidationHandlerNothing<NpgsqlCreates.DbWrapper,LogWrapper,ControllerWrapper>(),
            transformHandler: new TransformParse<ControllerWrapper>(),
            handler: new Handler.WebLoginTokenHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewWebLoginToken>(),
            input: new ControllerWrapper(this.ControllerHandlerBuild()),
            default
        );
        
        return TransactionToIResult(transaction);
    }

    [HttpGet("/api/webupdateCookie")]
    [PrivilegeRoute(route: "/api/webupdateCookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>))]
    public async Task<IActionResult> WebUpdateCookie() {
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new NpgsqlCreates(),
            loggerCreates: new LogCreates(),
            validationHandler: new ValidationHandlerNothing<NpgsqlCreates.DbWrapper,LogWrapper,ControllerWrapper>(),
            transformHandler: new TransformParse<ControllerWrapper>(),
            handler: new Handler.WebUpdateCookieHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewUpdateCookieInfo>(),
            input: new ControllerWrapper(this.ControllerHandlerBuild()),
            default
        );
        
        return TransactionToIResult(transaction);
    }


    [HttpPost("/api/webresetpasswdandsendemail")]
    [PrivilegeRoute(route: "/api/webresetpasswdandsendemail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewResetPasswdAndSendEmail>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswdAndSendEmail([FromBody] PostResetPasswdAndSendEmail prop) {
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new ResetPasswdAndSendEmailValidation(),
            transformHandler: new TransformAction<ControllerPostWrapper<PostResetPasswdAndSendEmail>,ControllerPostWrapper<ResetPasswdAndSendEmailDto>>(
                (i) => new ControllerPostWrapper<ResetPasswdAndSendEmailDto>(i.Controller,
                    DtoMapper.ResetPasswdAndSendEmailToDto(i.Post))),
            handler: new Handler.ResetPasswdAndSendEmailHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewResetPasswdAndSendEmail>(),
            input: new ControllerPostWrapper<PostResetPasswdAndSendEmail>(this.ControllerHandlerBuild(), prop),
            default
        );
        
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api/token/newpasswdwithtoken")]
    [PrivilegeRoute(route: "/api/token/newpasswdwithtoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewWebReplacePasswordWithToken>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetNewPasswd([FromBody] PostApi.PostApi2GroundNoHeader<PostSetNewPasswd> prop) {
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new SetNewPasswdValidation(),
            transformHandler: new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSetNewPasswd>>,
                ControllerPostWrapper<SetNewPasswdDto>>((i) => 
                new ControllerPostWrapper<SetNewPasswdDto>(i.Controller, DtoMapper.SetNewPasswdToDto(i.Post.Body!))),
            handler: new Handler.SetNewPasswdHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewWebReplacePasswordWithToken>(),
            input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSetNewPasswd>>(this.ControllerHandlerBuild(), prop),
            default
        );
        
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api/signin/patreon")]
    [PrivilegeRoute(route: "/api/signin/patreon")]
    public async Task<IActionResult> PatreonSignLogin([FromForm] string provider) {
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
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
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpGet("/api/signout/patreon")]
    [PrivilegeRoute(route: "/api/signout/patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatreonSignout() {
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
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
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }


    [HttpGet("/api/weblogout")]
    [PrivilegeRoute(route: "/api/weblogout")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveCookie() {
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new ValidationHandlerNothing<NpgsqlCreates.DbWrapper,LogWrapper,ControllerWrapper>(),
            transformHandler: new TransformParse<ControllerWrapper>(),
            handler: new Handler.RemoveCookieHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ApiTypes.ViewWork>(),
            input: new ControllerWrapper(this.ControllerHandlerBuild()),
            default
        );
        
        return TransactionToIResult(transaction);
    }
}










