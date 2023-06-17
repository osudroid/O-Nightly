using Microsoft.AspNetCore.Mvc;
using OsuDroid.Database.TableFn;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.Validate;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Dto;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class Profile : ControllerExtensions {
    /// <summary> Guid Token Id, long UserId </summary>
    /// <returns></returns>
    private static readonly TimeoutTokenDictionary<Guid, (long UserId, string Email)> _patreoneMailToken =
        new(TimeSpan.FromMinutes(5), 10000, 10000);

    private static readonly TimeoutTokenDictionary<Guid, (long UserId, string Email)> _deleteAccMailToken =
        new(TimeSpan.FromMinutes(5), 10000, 10000);

    [HttpGet("/api/profile/stats/{id:long}")]
    [PrivilegeRoute(route: "/api/profile/stats/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewProfileStats))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewProfileStats))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileStats([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var optionUserAndStatsResult = await log.AddResultAndTransformAsync(
                await Query.GetUserInfoAndBblUserStatsByUserIdAsync(db, userId));

            if (optionUserAndStatsResult == EResult.Err)
                return GetInternalServerError();
            if (optionUserAndStatsResult.Ok().IsNotSet())
                return Ok(new ViewProfileStats { Found = false });

            var userInfoAndStats = optionUserAndStatsResult.Ok().Unwrap();
            var rankOpt = (await log.AddResultAndTransformAsync(await Query
                    .GetUserRankAsync(db, userId, userInfoAndStats.OverallScore)))
                .OkOrDefault();
            if (rankOpt.IsNotSet())
                return GetInternalServerError();
        

            Option<Patron> optionBblPatron = Option<Patron>.Empty;
            if ((userInfoAndStats.Email??"").Length == 0) {
                optionBblPatron = (await log.AddResultAndTransformAsync(await QueryPatron
                        .GetByPatronEmailAsync(db, userInfoAndStats.Email ?? "")))
                    .OkOrDefault();
            }

            return Ok(new ViewProfileStats {
                Username = userInfoAndStats.Username,
                Id = userInfoAndStats.UserId,
                Found = true,
                OverallPlaycount = userInfoAndStats.OverallPlaycount,
                Region = userInfoAndStats.Region,
                Active = userInfoAndStats.Active,
                Supporter = optionBblPatron.IsSet() && optionBblPatron.Unwrap().ActiveSupporter,
                GlobalRanking = rankOpt.Unwrap().GlobalRank,
                CountryRanking = rankOpt.Unwrap().CountryRank,
                OverallScore = userInfoAndStats.OverallScore,
                OverallAccuracy = userInfoAndStats.OverallAccuracy,
                OverallCombo = userInfoAndStats.OverallCombo,
                OverallXss = userInfoAndStats.OverallXss,
                OverallSs = userInfoAndStats.OverallSs,
                OverallXs = userInfoAndStats.OverallXs,
                OverallS = userInfoAndStats.OverallS,
                OverallA = userInfoAndStats.OverallA,
                OverallB = userInfoAndStats.OverallB,
                OverallC = userInfoAndStats.OverallC,
                OverallD = userInfoAndStats.OverallD,
                OverallHits = userInfoAndStats.OverallHits,
                OverallPerfect = userInfoAndStats.OverallPlaycount,
                Overall300 = userInfoAndStats.Overall300,
                Overall100 = userInfoAndStats.Overall100,
                Overall50 = userInfoAndStats.Overall50,
                OverallGeki = userInfoAndStats.OverallGeki,
                OverallKatu = userInfoAndStats.OverallKatu,
                OverallMiss = userInfoAndStats.OverallMiss,
                RegistTime = userInfoAndStats.RegisterTime,
                LastLoginTime = userInfoAndStats.LastLoginTime
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/stats/timeline/{id:long}")]
    [PrivilegeRoute(route: "/api/profile/stats/timeline/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUserRankTimeLine))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> WebProfileStatsTimeLine([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (userId < 0)
                return BadRequest();

            var rankingTimeline = log.AddResultAndTransform(await QueryGlobalRankingTimeline
                                         .BuildTimeLineAsync(db, userId, DateTime.UtcNow - TimeSpan.FromDays(90)))
                                     .OkOr(Array.Empty<Entities.GlobalRankingTimeline>())
                                     .Select(x => new ViewUserRankTimeLine.RankTimeLineValue {
                                         Date = x.Date,
                                         Score = x.Score,
                                         Rank = x.GlobalRanking
                                     }).ToList();

            return Ok(new ViewUserRankTimeLine {
                UserId = userId,
                List = rankingTimeline
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/topplays/{id:long}")]
    [PrivilegeRoute(route: "/api/profile/topplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopPlays([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var result = await log.AddResultAndTransformAsync(await QueryPlayScore.GetTopScoreFromUserIdAsync(db, userId));
            if (result == EResult.Err)
                return GetInternalServerError();
            
            return Ok(new ViewPlays {
                Found = true,
                Scores = result.Ok()
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/topplays/{id:long}/page/{page:int}")]
    [PrivilegeRoute(route: "/api/profile/topplays/{id:long}/page/{page:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopPlaysPage([FromRoute(Name = "id")] long userId, int page) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (long.IsNegative(userId))
                return BadRequest("userid Is Negative");
            if (int.IsNegative(page))
                return BadRequest("page Is Negative");
            
            var fetchResult = await log.AddResultAndTransformAsync(
                await QueryPlayScore.GetTopScoreFromUserIdWithPageAsync(db, userId, page, 50));
            if (fetchResult == EResult.Err)
                return GetInternalServerError();

            var scores = fetchResult.Ok();
            return Ok(new ViewPlays() { Found = true, Scores = scores });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/recentplays/{id:long}")]
    [PrivilegeRoute(route: "/api/profile/recentplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewPlays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopRecent([FromRoute(Name = "id")] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var result = await log.AddResultAndTransformAsync(
                await QueryPlayScore.GetLastPlayScoreFilterByUserIdAsync(db, userId, 50));

            if (result == EResult.Err)
                return GetInternalServerError();

            return Ok(new ViewPlays {
                Found = true,
                Scores = result.Ok()
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/profile/update/email")]
    [PrivilegeRoute(route: "/api/profile/update/email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEmail([FromBody] PostUpdateEmail prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.AnyValidate() == EResult.Err)
                return Ok(new ApiTypes.Work { HasWork = false });

            var cookieToken = this.LoginTokenInfo(db).Ok().Unwrap();


            var userInfoResult = await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId));
            if (userInfoResult == EResult.Err)
                return GetInternalServerError();

            if (userInfoResult.Ok().IsNotSet())
                return Ok(new ApiTypes.Work { HasWork = false });

            var userInfo = userInfoResult.Ok().Unwrap();
            
            if (Database.TableFn.BblUser.PasswordEqual(userInfo, prop.Passwd ?? "") == false)
                return Ok(new ApiTypes.Work { HasWork = false });

            if (prop.OldEmail != userInfo.Email)
                return Ok(new ApiTypes.Work { HasWork = false });


            var updateResult = await log.AddResultAndTransformAsync<string>(
                await QueryUserInfo.UpdatePasswordAsync(db, userInfo.UserId, prop.NewEmail ?? ""));
            
            if (updateResult == EResult.Err)
                return GetInternalServerError();
            return Ok(new ApiTypes.Work { HasWork = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/profile/update/passwd")]
    [PrivilegeRoute(route: "/api/profile/update/passwd")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePasswd([FromBody] PostUpdatePasswd prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.AnyValidate() == EResult.Err)
                return Ok(new ApiTypes.Work { HasWork = false });

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            
            
            var userInfoOption = (await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetByUserIdAsync(db, cookieInfo.UserId)))
                .OkOrDefault();
            if (userInfoOption.IsNotSet())
                return GetInternalServerError();

            var userInfo = userInfoOption.Unwrap();
            var newPasswdHash = MD5.Hash(prop.NewPasswd + Setting.Password_Seed!.Value).ToLower();
            var oldPasswdHash = MD5.Hash(prop.OldPasswd + Setting.Password_Seed!.Value).ToLower();
            if (userInfo.Password != oldPasswdHash)
                return Ok(new ApiTypes.Work { HasWork = false });
            
            var result = await log.AddResultAndTransformAsync<string>(
                await QueryUserInfo.UpdatePasswordByUserIdAsync(db, cookieInfo.UserId, newPasswdHash));
            if (result == EResult.Err)
                return GetInternalServerError();
            return Ok(new ApiTypes.Work { HasWork = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/profile/update/username")]
    [PrivilegeRoute(route: "/api/profile/update/username")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewUpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUsername([FromBody] PostUpdateUsername prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            prop.NewUsername = prop.NewUsername?.Trim();
            if (prop.AnyValidate() == EResult.Err)
                return Ok(new ViewUpdateUsernameRes { HasWork = false });

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();

            var userInfoOption = (await log.AddResultAndTransformAsync(await QueryUserInfo
                .GetByUserIdAsync(db, cookieInfo.UserId))).OkOrDefault();

            if (userInfoOption.IsNotSet())
                return GetInternalServerError();

            var userInfo = userInfoOption.Unwrap();

            if (BblUser.PasswordEqual(userInfo, prop.Passwd ?? "") == false
                || (userInfo.Username ?? "").ToLower() != (prop.OldUsername ?? "").ToLower()
               ) {
                return Ok(new ViewUpdateUsernameRes { HasWork = false });
            }

            var checkUsername = await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetByUsernameAsync(db, prop.NewUsername??""));

            if (checkUsername == EResult.Err)
                return GetInternalServerError();
            
            if (checkUsername.Ok().IsSet())
                return Ok(new ApiTypes.Work { HasWork = false }); 
            
            var result = await log.AddResultAndTransformAsync<string>(
                await QueryUserInfo.UpdateUsernameByUserIdAsync(db, userInfo.UserId, prop.NewUsername ?? ""));

            if (result == EResult.Err)
                return GetInternalServerError();
            
            return Ok(new ApiTypes.Work { HasWork = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/profile/update/avatar")]
    [PrivilegeRoute(route: "/api/profile/update/avatar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewUpdateAvatar))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAvatar([FromBody] PostUpdateAvatar prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.AnyValidate() == EResult.Err)
                return Ok(new ViewUpdateAvatar { PasswdFalse = true });

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            
            {
                var userInfoOption = (await log.AddResultAndTransformAsync(await QueryUserInfo
                    .GetByUserIdAsync(db, cookieInfo.UserId))).OkOrDefault();

                if (userInfoOption.IsNotSet())
                    return GetInternalServerError();

                var userInfo = userInfoOption.Unwrap();

                if (BblUser.PasswordEqual(userInfo, prop.Passwd ?? "") == false) {
                    return Ok(new ViewUpdateAvatar { PasswdFalse = true });
                }
            }
            
            
            byte[] imageBytes = Array.Empty<byte>();
            try {
                var charArr = prop.ImageBase64.AsSpan(prop.ImageBase64!.IndexOf(',') + 1).ToArray();
                // TODO Write one Convert.FromBase64String With Span
                imageBytes = Convert.FromBase64CharArray(charArr, 0, charArr.Length);
            }
            catch (Exception e) {
                await log.AddLogErrorAsync(e.ToString());
                return BadRequest();
            }

            var result = await log.AddResultAndTransformAsync<string>(
                await OsuDroidLib.Lib.UserAvatarHandler.UpdateImageForUserAsync(db, cookieInfo.UserId, imageBytes));

            if (result == EResult.Err)
                return GetInternalServerError();
            
            return Ok(new ViewUpdateAvatar {
                PasswdFalse = false,
                ImageToBig = false,
                IsNotAImage = false
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/profile/update/patreonemail")]
    [PrivilegeRoute(route: "/api/profile/update/patreonemail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePatreonEmail([FromBody] PostUpdatePatreonEmail prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.AnyValidate() == EResult.Err)
                return Ok(ApiTypes.Work.False);

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            var passwdHash = BblUser.HashPasswd(prop.Passwd ?? "", Setting.Password_Seed!.Value);

            var result = await QueryUserInfo.CheckPasswordGetIdAndUsernameAsync(db, passwdHash);
            if (result == EResult.Err)
                return GetInternalServerError();
            if (result.Ok().IsNotSet())
                return Ok(new ApiTypes.Work { HasWork = false });
            var userInfo = result.Ok().Unwrap();
            var token = Guid.NewGuid();

            _patreoneMailToken.Add(token, (cookieInfo.UserId, prop.Email!));

            SendEmail.MainSendPatreonVerifyLinkToken(userInfo.Username!, userInfo.Email!, token);

            return Ok(new ApiTypes.Work { HasWork = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/accept/patreonemail/token/{token:guid}")]
    [PrivilegeRoute(route: "/api/profile/accept/patreonemail/token/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AcceptPatreonEmail([FromRoute(Name = "token")] Guid token) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (token == Guid.Empty)
                return Ok(new ApiTypes.Work { HasWork = false });

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            
            var response = _patreoneMailToken.Pop(token);

            if (response.IsNotSet()) {
                await log.AddLogDebugAsync($"Token Not Found: {token}");
                return Ok(new ApiTypes.Work { HasWork = false });
            }

            var userIdAndEmail = response.Unwrap();
            
            if ((await log.AddResultAndTransformAsync<string>(
                    await QueryUserInfo.SetPatreonEmailAsync(db, userIdAndEmail.UserId, userIdAndEmail.Email))) == EResult.Err)
                return GetInternalServerError();
            
            if ((await log.AddResultAndTransformAsync<string>(
                    await QueryUserInfo.SetAcceptPatreonEmailAsync(db, userIdAndEmail.UserId))) == EResult.Err)
                return GetInternalServerError();

            return Ok(new ApiTypes.Work { HasWork = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/profile/drop-account/sendMail")]
    [PrivilegeRoute(route: "/api/profile/drop-account/sendMail}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewCreateDropAccountTokenRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDropAccountToken([FromBody] ApiTypes.Api2GroundNoHeader<PostCreateDropAccountToken> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return Ok(ViewCreateDropAccountTokenRes.HasElseError());

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            
            var optionBblUserResult = await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetByUserIdAsync(db, cookieInfo.UserId));

            if (optionBblUserResult == EResult.Err)
                return GetInternalServerError();
            if (optionBblUserResult.Ok().IsNotSet())
                return Ok(ViewCreateDropAccountTokenRes.HasElseError());

            var userInfo = optionBblUserResult.Ok().Unwrap();
            if (userInfo.Password != this.ToPasswdHash(prop.Body!.Password))
                return Ok(ViewCreateDropAccountTokenRes.PasswordIsFalse());


            var deleteAccToken = Guid.NewGuid();
            _deleteAccMailToken.Add(deleteAccToken, (userInfo.UserId, userInfo.Email??""));
            Utils.SendEmail.MainSendDropAccountVerifyLinkToken(userInfo.Username??"", userInfo.Email??"", deleteAccToken);

            return Ok(ViewCreateDropAccountTokenRes.NoError());
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/drop-account/token/{token:guid}")]
    [PrivilegeRoute(route: "/api/profile/drop-account/token/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DropAccountWithTokenAsync([FromRoute(Name = "token")] Guid token) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            _deleteAccMailToken.CleanDeadTokens();

            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            var deleteAccTokenResponse = _deleteAccMailToken.Pop(token: token);
            if (deleteAccTokenResponse.IsNotSet())
                return BadRequest();

            var userId = deleteAccTokenResponse.Unwrap().UserId;
            if (userId == cookieInfo.UserId)
                return BadRequest();
        
            var deleteAccountResponse = await log.AddResultAndTransformAsync<string>(
                await QueryUserInfo.DeleteAsync(db, userId));
            if (deleteAccountResponse == EResult.Err)
                return GetInternalServerError();
            
            return Ok(ApiTypes.Work.True);
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }


    [HttpGet("/api/profile/top-play-by-marks-length/user-id/{userId:long}")]
    [PrivilegeRoute(route: "/api/profile/top-play-by-marks-length/user-id/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlaysMarksLength))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebProfileTopPlaysByMarksLength([FromRoute] long userId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (long.IsNegative(userId)) return BadRequest("UserId < 0");

            
            var result = await log.AddResultAndTransformAsync(
                await QueryPlayScore.CountMarkPlaysByUserIdAsync(db, userId));
            if (result == EResult.Err)
                return GetInternalServerError();
            Dictionary<PlayScoreDto.EPlayScoreMark, long> dic = result.Ok();
            
            return Ok(ViewPlaysMarksLength.Factory(dic));
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}")]
    [PrivilegeRoute(route: "/api/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewPlays))]
    public async Task<IActionResult> WebProfileTopPlaysByMark(
        [FromRoute] long userId, [FromRoute] string markString, [FromRoute] int page) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (long.IsNegative(userId))
                return BadRequest("userid Is Negative");
            if (string.IsNullOrEmpty(markString))
                return BadRequest("markString Is Null Or Empty");
            if (int.IsNegative(page))
                return BadRequest("page Is Negative");

            
            if (EPlayScoreMarkExtensions.TryParse(markString, out var mark)) 
                return BadRequest("markString Case Not Exist");

            
            var fetchResult = await log.AddResultAndTransformAsync(
                await QueryPlayScore.GetTopScoreFromUserIdFilterMark(db, userId, page, 50, mark));
            if (fetchResult == EResult.Err)
                return GetInternalServerError();

            var scores = fetchResult.Ok();
            return Ok(new ViewPlays() { Found = true, Scores = scores });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }




}