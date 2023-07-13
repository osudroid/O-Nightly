using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroid.Model;
using OsuDroidLib.Dto;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api2;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Api2Profile : ControllerExtensions {
    [HttpGet("/api2/profile/stats/{id:long}")]
    [PrivilegeRoute(route: "/api2/profile/stats/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewProfileStats))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewProfileStats))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileStats([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;
        
        try {
            var optionUserAndStatsResult = await log.AddResultAndTransformAsync(
                await Query.GetUserInfoAndBblUserStatsByUserIdAsync(db, userId));

            if (optionUserAndStatsResult == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            if (optionUserAndStatsResult.Ok().IsNotSet())
                return Ok(new ViewProfileStats { Found = false });


            var result = await log.AddResultAndTransformAsync(
                await ModelApi2Profile.WebProfileStatsAsync(this, db, log, userId));

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

    [HttpGet("/api2/profile/stats/timeline/{id:long}")]
    [PrivilegeRoute(route: "/api2/profile/stats/timeline/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUserRankTimeLine))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> WebProfileStatsTimeLine([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (userId < 0)
                return await RollbackAndGetBadRequestAsync(dbT);

            var result = await log.AddResultAndTransformAsync(
                await ModelApi2Profile.WebProfileStatsTimeLineAsync(this, db, userId));

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

    [HttpGet("/api2/profile/topplays/{id:long}")]
    [PrivilegeRoute(route: "/api2/profile/topplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopPlays([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            var result =
                await log.AddResultAndTransformAsync(await QueryPlayScore.GetTopScoreFromUserIdAsync(db, userId));
            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            
            
            return Ok(new ViewPlays {
                Found = true,
                Scores = result.Ok().Select(ViewPlayScore.FromPlayScore).ToList()
            });
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

    [HttpGet("/api2/profile/topplays/{id:long}/page/{page:int}")]
    [PrivilegeRoute(route: "/api2/profile/topplays/{id:long}/page/{page:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopPlaysPage([FromRoute(Name = "id")] long userId, int page) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (long.IsNegative(userId))
                return await RollbackAndGetBadRequestAsync(dbT, "userid Is Negative");
            if (int.IsNegative(page))
                return await RollbackAndGetBadRequestAsync(dbT, "page Is Negative");

            var fetchResult = await log.AddResultAndTransformAsync(
                await QueryPlayScore.GetTopScoreFromUserIdWithPageAsync(db, userId, page, 50));
            if (fetchResult == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

            return Ok(new ViewPlays() { Found = true, Scores = fetchResult.Ok().Select(ViewPlayScore.FromPlayScore).ToList() });
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

    [HttpGet("/api2/profile/recentplays/{id:long}")]
    [PrivilegeRoute(route: "/api2/profile/recentplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopRecent([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            var result = await log.AddResultAndTransformAsync(
                await QueryPlayScore.GetLastPlayScoreFilterByUserIdAsync(db, userId, 50));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            
            return Ok(new ViewPlays {
                Found = true,
                Scores = result.Ok().Select(ViewPlayScore.FromPlayScore).ToList()
            });
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

    [HttpPost("/api2/profile/update/email")]
    [PrivilegeRoute(route: "/api2/profile/update/email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEmail([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdateEmail> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (prop.ValuesAreGood() == false) {
                await log.AddLogDebugAsync("Post Prop Are Bad");
                return await RollbackAndGetBadRequestAsync(dbT);
            }

            var body = prop.Body!;

            UserIdAndToken cookieToken = this.LoginTokenInfo(db).Ok().Unwrap();


            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile
                .UpdateEmailAsync(this, db, log, DtoMapper.UpdateEmailToDto(body), cookieToken));

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

    [HttpPost("/api2/profile/update/passwd")]
    [PrivilegeRoute(route: "/api2/profile/update/passwd")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePasswd([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdatePasswd> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (!prop.ValuesAreGood())
                return Ok(new ApiTypes.ViewWork { HasWork = false });

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile
                .UpdatePasswdAsync(this, db, DtoMapper.UpdatePasswdToDto(prop.Body!), cookieInfo));

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

    [HttpPost("/api2/profile/update/username")]
    [PrivilegeRoute(route: "/api2/profile/update/username")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewUpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult>
        UpdateUsername([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdateUsername> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (prop.Body is null || prop.Body.NewUsername is null)
                return await this.RollbackAndGetBadRequestAsync(dbT, "Post Body Is Bad");

            prop.Body.NewUsername = prop.Body.NewUsername.Trim();
            if (!prop.ValuesAreGood())
                return await this.RollbackAndGetBadRequestAsync(dbT, "Post Body Is Bad");

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile
                .UpdateUsernameAsync(this, db, DtoMapper.UpdateUsernameToDto(prop.Body!), cookieInfo));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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

    [HttpPost("/api2/profile/update/avatar")]
    [PrivilegeRoute(route: "/api2/profile/update/avatar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUpdateAvatar))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAvatar([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdateAvatar> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (!prop.ValuesAreGood())
                return await this.RollbackAndGetBadRequestAsync(dbT, "Post Body Is Bad");

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile
                .UpdateAvatarAsync(this, db, DtoMapper.UpdateAvatarToDto(prop.Body!), cookieInfo));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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

    [HttpPost("/api2/profile/update/patreonemail")]
    [PrivilegeRoute(route: "/api2/profile/update/patreonemail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePatreonEmail(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (!prop.ValuesAreGood())
                return await this.RollbackAndGetBadRequestAsync(dbT, "Post Body Is Bad");

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile
                .UpdatePatreonEmailAsync(this, db, DtoMapper.UpdatePatreonEmailToDto(prop.Body!), cookieInfo));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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

    [HttpGet("/api2/profile/accept/patreonemail/token/{token:guid}")]
    [PrivilegeRoute(route: "/api2/profile/accept/patreonemail/token/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AcceptPatreonEmail([FromRoute(Name = "token")] Guid token) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (token == Guid.Empty)
                return Ok(new ApiTypes.ViewWork { HasWork = false });

            var result = await log.AddResultAndTransformAsync(
                await ModelApi2Profile.AcceptPatreonEmailAsync(this, db, token));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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

    [HttpPost("/api2/profile/drop-account/sendMail")]
    [PrivilegeRoute(route: "/api2/profile/drop-account/sendMail}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCreateDropAccountTokenRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDropAccountToken(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostCreateDropAccountToken> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (prop.ValuesAreGood() == false)
                return Ok(ViewCreateDropAccountTokenRes.HasElseError());

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile.CreateDropAccountTokenAsync(
                this, db, DtoMapper.CreateDropAccountTokenToDto(prop.Body!), cookieInfo));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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

    [HttpGet("/api2/profile/drop-account/token/{token:guid}")]
    [PrivilegeRoute(route: "/api2/profile/drop-account/token/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DropAccountWithTokenAsync([FromRoute(Name = "token")] Guid token) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (token == Guid.Empty)
                return await this.RollbackAndGetBadRequestAsync(dbT, "Token Is Empty");

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile.DropAccountWithTokenAsync(
                this, db, token, cookieInfo));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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


    [HttpGet("/api2/profile/top-play-by-marks-length/user-id/{userId:long}")]
    [PrivilegeRoute(route: "/api2/profile/top-play-by-marks-length/user-id/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlaysMarksLength))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopPlaysByMarksLength([FromRoute] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (long.IsNegative(userId))
                return await RollbackAndGetBadRequestAsync(dbT, "UserId < 0");


            var result = await log.AddResultAndTransformAsync(await ModelApi2Profile
                .WebProfileTopPlaysByMarksLengthAsync(this, db, userId));

            if (result == EResult.Err)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

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

    [HttpGet("/api2/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}")]
    [PrivilegeRoute(
        route: "/api2/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    public async Task<IActionResult> WebProfileTopPlaysByMark(
        [FromRoute] long userId, [FromRoute] string markString, [FromRoute] int page) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;

        try {
            if (long.IsNegative(userId))
                return await RollbackAndGetBadRequestAsync(dbT, "userid Is Negative");
            if (string.IsNullOrEmpty(markString))
                return await RollbackAndGetBadRequestAsync(dbT, "markString Is Null Or Empty");
            if (int.IsNegative(page))
                return await RollbackAndGetBadRequestAsync(dbT, "page Is Negative");


            if (EPlayScoreMarkExtensions.TryParse(markString, out var mark))
                return await RollbackAndGetBadRequestAsync(dbT, "markString Case Not Exist");


            var fetchResult = await log.AddResultAndTransformAsync(
                await QueryPlayScore.GetTopScoreFromUserIdFilterMark(db, userId, page, 50, mark));
            if (fetchResult == EResult.Err)
                return await RollbackAndGetBadRequestAsync(dbT);

            var scores = fetchResult.Ok().Select(ViewPlayScore.FromPlayScore).ToList();;
            return Ok(new ViewPlays() { Found = true, Scores = scores });
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























