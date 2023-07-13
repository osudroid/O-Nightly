using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Lib.Validate;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroid.Model;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api;

#nullable enable

public sealed class Login : ControllerExtensions {
    [HttpPost("/api/weblogin")]
    [PrivilegeRoute(route: "/api/weblogin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLogin))]
    public async Task<IActionResult> WebLogin([FromBody] PostWebLogin prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (!prop.ValuesAreGood()) {
                await log.AddLogDebugAsync("Post Prop Are Not Valid");
                isComplete = true;
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var result = await log.AddResultAndTransformAsync(
                await Model.ModelApiLogin.WebLogin(this, db, DtoMapper.WebLoginToDto(prop)));

            if (result == EResult.Err) {
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLogin))]
    [HttpPost("/api/webloginwithusername")]
    [PrivilegeRoute(route: "/api/webloginwithusername")]
    public async Task<IActionResult> WebLoginWithUsername([FromBody] PostWebLoginWithUsername prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            prop.Username = this.FixUsername(prop.Username ?? string.Empty);

            if (!prop.ValuesAreGood()) {
                isComplete = true;
                await log.AddLogDebugAsync("Post Prop Are Not Valid");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var result = await log.AddResultAndTransformAsync(await ModelApiLogin.WebLoginWithUsername(
                this,
                db,
                DtoMapper.WebLoginWithUsernameToDto(prop)));

            if (result == EResult.Err) {
                isComplete = true;
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpPost("/api/webregister")]
    [PrivilegeRoute(route: "/api/webregister")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebRegister([FromBody] PostWebRegister prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            prop.Username = this.FixUsername(prop.Username ?? string.Empty);

            if (!prop.ValuesAreGood()) {
                isComplete = true;
                await log.AddLogDebugAsync("Post Prop Are Not Valid");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var result = await log.AddResultAndTransformAsync(
                await ModelApiLogin.WebRegisterAsync(this, db, DtoMapper.WebRegisterToDto(prop)));

            if (result == EResult.Err) {
                isComplete = true;
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpGet("/api/weblogintoken")]
    [PrivilegeRoute(route: "/api/weblogintoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLoginToken))]
    public async Task<IActionResult> WebLoginToken() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            var result = await log.AddResultAndTransformAsync(await ModelApiLogin.WebLoginTokenAsync(this, db));

            if (result == EResult.Err) {
                isComplete = true;
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpGet("/api/webupdateCookie")]
    [PrivilegeRoute(route: "/api/webupdateCookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>))]
    public async Task<IActionResult> WebUpdateCookie() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            UserIdAndToken cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            this.AppendCookie(ECookie.LoginCookie, cookieInfo.Token.ToString());


            var result = await log.AddResultAndTransformAsync(
                await ModelApiLogin.WebUpdateCookieAsync(this, db, cookieInfo));

            if (result == EResult.Err) {
                isComplete = true;
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }


    [HttpPost("/api/webresetpasswdandsendemail")]
    [PrivilegeRoute(route: "/api/webresetpasswdandsendemail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewResetPasswdAndSendEmail))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewResetPasswdAndSendEmail))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswdAndSendEmail([FromBody] PostResetPasswdAndSendEmail prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (!prop.ValuesAreGood()) {
                isComplete = true;
                await log.AddLogDebugAsync("Post Prop Are Not Valid");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            Option<IPAddress> optionIpAddress =
                Option<IPAddress>.Trim(await log.AddResultAndTransformAsync(GetIpAddress()));
            if (optionIpAddress.IsNotSet()) {
                isComplete = true;
                return await this.RollbackAndGetBadRequestAsync(dbT, "Can Not Get IP IS NEEDED");
            }

            IPAddress ipAddress = optionIpAddress.Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApiLogin.ResetPasswdAndSendEmailAsync(
                this, db, DtoMapper.ResetPasswdAndSendEmailToDto(prop), ipAddress));

            if (result == EResult.Err) {
                isComplete = true;
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpPost("/api/token/newpasswdwithtoken")]
    [PrivilegeRoute(route: "/api/token/newpasswdwithtoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebReplacePasswordWithToken))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewWebReplacePasswordWithToken))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetNewPasswd([FromBody] PostApi.PostApi2GroundNoHeader<PostSetNewPasswd> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (!prop.ValuesAreGood()) {
                isComplete = true;
                await log.AddLogDebugAsync("Post Prop Are Not Valid");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var result = await log.AddResultAndTransformAsync(await ModelApiLogin.SetNewPasswdAsync(
                this, db, DtoMapper.SetNewPasswdToDto(prop.Body!)));

            if (result == EResult.Err) {
                isComplete = true;
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }

    [HttpPost("/api/signin/patreon")]
    [PrivilegeRoute(route: "/api/signin/patreon")]
    public async Task<IActionResult> PatreonSignLogin([FromForm] string provider) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
                return await this.RollbackAndGetBadRequestAsync(dbT, "provider IsNullOrWhiteSpace");

            if (await HttpContext.IsProviderSupportedAsync(provider))
                return await this.RollbackAndGetBadRequestAsync(dbT);

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
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
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatreonSignout() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            // TODO PatreonSignout


            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return Ok();
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }


    [HttpGet("/api/weblogout")]
    [PrivilegeRoute(route: "/api/weblogout")]
    public async Task<IActionResult> RemoveCookie() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;
        
        try {
            RemoveCookieByEName(ECookie.LoginCookie);
            return Ok();
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }
}










