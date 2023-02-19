using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using OsuDroid.Database.TableFn;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Lib.Validate;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using BblGlobalRankingTimeline = OsuDroid.Database.TableFn.BblGlobalRankingTimeline;
using BblPatron = OsuDroidLib.Database.Entities.BblPatron;
using BblScore = OsuDroidLib.Database.Entities.BblScore;
using BblUser = OsuDroidLib.Database.Entities.BblUser;

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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProfileStats))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProfileStats))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileStats([FromRoute(Name = "id")] long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        var sql = new Sql(
            "SELECT * FROM public.bbl_user JOIN bbl_user_stats bus on bus.uid = bbl_user.id WHERE id = @0", userId);
        
        var optionUserAndStats = log.AddResultAndTransform(db.SingleOrDefault<BblUserAndBblUserStats>(sql))
            .Map(x => Option<BblUserAndBblUserStats>.NullSplit(x))
            .OkOr(Option<BblUserAndBblUserStats>.Empty);

        if (optionUserAndStats.IsSet() == false)
            return Ok(new ProfileStats { Found = false });

        BblUserAndBblUserStats userAndStats = optionUserAndStats.Unwrap();
        
        var sqlRank = new Sql(@$"
SELECT t.global_rank as global_rank, t.country_rank as country_rank
FROM (
         SELECT uid,
                rank() OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as global_rank,
                rank() OVER (PARTITION BY bu.region ORDER BY overall_score DESC, bu.last_login_time DESC) as country_rank
         FROM bbl_user_stats
                  FULL JOIN bbl_user bu on bu.id = bbl_user_stats.uid
         WHERE region IS NOT NULL
         AND overall_score >= {userAndStats.OverallScore} AND banned = false
     ) as t
WHERE uid = {userId}
");

        List<BblUser.UserRank> userRank = log.AddResultAndTransform(db.Fetch<BblUser.UserRank>(sqlRank)).OkOr(new());
        
        Option<BblPatron> optionBblPatron = Option<BblPatron>.Empty;
        if ((userAndStats.Email??"").Length == 0) {
            optionBblPatron = Option<BblPatron>.Transform(log.AddResultAndTransform(
                Database.TableFn.BblUser.GetBblPatron(new BblUser { PatronEmail = userAndStats.Email }, db)));
        }

        return Ok(new ProfileStats {
            Username = userAndStats.Username,
            Id = userAndStats.Id,
            Found = true,
            OverallPlaycount = userAndStats.OverallPlaycount,
            Region = userAndStats.Region,
            Active = userAndStats.Active,
            Supporter = optionBblPatron.IsSet() && optionBblPatron.Unwrap().ActiveSupporter,
            GlobalRanking = userRank[0].globalRank,
            CountryRanking = userRank[0].CountryRank,
            OverallScore = userAndStats.OverallScore,
            OverallAccuracy = userAndStats.OverallAccuracy,
            OverallCombo = userAndStats.OverallCombo,
            OverallXss = userAndStats.OverallXss,
            OverallSs = userAndStats.OverallSs,
            OverallXs = userAndStats.OverallXs,
            OverallS = userAndStats.OverallS,
            OverallA = userAndStats.OverallA,
            OverallB = userAndStats.OverallB,
            OverallC = userAndStats.OverallC,
            OverallD = userAndStats.OverallD,
            OverallHits = userAndStats.OverallHits,
            OverallPerfect = userAndStats.OverallPerfect,
            Overall300 = userAndStats.Overall300,
            Overall100 = userAndStats.Overall100,
            Overall50 = userAndStats.Overall50,
            OverallGeki = userAndStats.OverallGeki,
            OverallKatu = userAndStats.OverallKatu,
            OverallMiss = userAndStats.OverallMiss,
            RegistTime = userAndStats.RegistTime,
            LastLoginTime = userAndStats.LastLoginTime
        });
    }

    [HttpGet("/api/profile/stats/timeline/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserRankTimeLine))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult WebProfileStatsTimeLine([FromRoute(Name = "id")] long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (userId < 0)
            return BadRequest();
        
        var rankingTimeline = log.AddResultAndTransform(BblGlobalRankingTimeline
            .BuildTimeLine(db, userId, DateTime.UtcNow - TimeSpan.FromDays(90)))
            .OkOr(Array.Empty<Entities.BblGlobalRankingTimeline>())
            .Select(x => new UserRankTimeLine.RankTimeLineValue {
                Date = x.Date,
                Score = x.Score,
                Rank = x.GlobalRanking
            }).ToList();
        
        return Ok(new UserRankTimeLine {
            UserId = userId,
            List = rankingTimeline
        });
    }

    [HttpGet("/api/profile/topplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileTopPlays([FromRoute(Name = "id")] long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        return Ok(new Plays {
            Found = true,
            Scores = Database.TableFn.BblScore.GetTopScoreFromUserId(db, userId).OkOr(new List<BblScore>(0)) 
        });
    }

    [HttpGet("/api/profile/topplays/{id:long}/page/{page:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileTopPlaysPage([FromRoute(Name = "id")] long userId, int page) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();

        if (long.IsNegative(userId))
            return BadRequest("userid Is Negative");
        if (int.IsNegative(page))
            return BadRequest("page Is Negative");
        
        var fetchResult = log.AddResultAndTransform(OsuDroid.Database.TableFn.BblScore.GetTopScoreFromUserIdWithPage(db, userId, page, 50));
        if (fetchResult == EResult.Err)
            return GetInternalServerError();

        var scores = fetchResult.Ok();
        return Ok(new Plays() { Found = true, Scores = scores });
    }
    
    [HttpGet("/api/profile/recentplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileTopRecent([FromRoute(Name = "id")] long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        var sql = new Sql(@$"
SELECT * FROM bbl_score 
WHERE uid = {userId}
ORDER BY bbl_score.id DESC 
LIMIT 50;
");
        
        return Ok(new Plays {
            Found = true,
            Scores = db.Fetch<BblScore>(sql).OkOrDefault() ?? new List<BblScore>(0)
        });
    }

    [HttpPost("/api/profile/update/email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateEmail([FromBody] UpdateEmailProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.AnyValidate() == EResult.Err)
            return Ok(new ApiTypes.Work { HasWork = false });
        
        var tokenInfoResp = log.AddResultAndTransform(LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        if (tokenInfoResp.IsSet() == false) return Ok(ApiTypes.Work.False);

        var userId = tokenInfoResp.Unwrap().UserId;

        var bblUser = log.AddResultAndTransform(db
                .SingleOrDefault<BblUser>($"SELECT email, password FROM bbl_user WHERE id = {userId}"))
            .OkOrDefault();

        if (bblUser is null)
            return Ok(new ApiTypes.Work { HasWork = false });

        if (Database.TableFn.BblUser.PasswordEqual(bblUser, prop.Passwd ?? "") == false)
            return Ok(new ApiTypes.Work { HasWork = false });

        if (prop.OldEmail != bblUser.Email)
            return Ok(new ApiTypes.Work { HasWork = false });

        var filePathOld = $"{Env.AvatarPath}/{MD5.Hash(bblUser.Email ?? "")}";
        var filePathNew = $"{Env.AvatarPath}/{MD5.Hash(prop.NewEmail ?? "")}";
        try {
            System.IO.File.Move(filePathOld, filePathNew);
        }
        catch(Exception e) {
            log.AddLogError(e.ToString());
        }

        if (log.AddResultAndTransform(
                db.Execute(@$"UPDATE bbl_user SET email = @0 WHERE id = {userId}", prop.NewEmail!)) == EResult.Err) {
            return Ok(ApiTypes.Work.False);
        }
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/update/passwd")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdatePasswd([FromBody] UpdatePasswdProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.AnyValidate() == EResult.Err)
            return Ok(new ApiTypes.Work { HasWork = false });

        var optiontokenInfoResp = log.AddResultAndTransform(LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        if (optiontokenInfoResp.IsSet() == false) 
            return Ok(ApiTypes.Work.False);

        var userId = optiontokenInfoResp.Unwrap().UserId;

        var bblUser = log.AddResultAndTransform(db
            .SingleOrDefault<BblUser>($"SELECT password FROM bbl_user WHERE id = {userId}")).OkOrDefault();
        if (bblUser is null)
            return Ok(new ApiTypes.Work { HasWork = false });

        var newPasswdHash = MD5.Hash(prop.NewPasswd + Env.PasswdSeed).ToLower();
        var oldPasswdHash = MD5.Hash(prop.OldPasswd + Env.PasswdSeed).ToLower();
        if (bblUser.Password != oldPasswdHash)
            return Ok(new ApiTypes.Work { HasWork = false });

        log.AddResultAndTransform(db.Execute(@$"UPDATE bbl_user SET password = @0 WHERE id = {userId}", newPasswdHash!));
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/update/username")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateUsername([FromBody] UpdateUsernameProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        prop.NewUsername = prop.NewUsername?.Trim();
        if (prop.AnyValidate() == EResult.Err)
            return Ok(new UpdateUsernameRes { HasWork = false });
        
        var optionTokenInfo = log.AddResultAndTransform(LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        if (optionTokenInfo.IsSet() == false) return Ok(new UpdateUsernameRes { HasWork = false });

        var userId = optionTokenInfo.Unwrap().UserId;

        var bblUser =
            log.AddResultAndTransform(db.SingleOrDefault<BblUser>(
                    $"SELECT username, password, username_last_change FROM bbl_user WHERE id = {userId}"))
                .OkOrDefault();

        if (bblUser is null
            || Database.TableFn.BblUser.PasswordEqual(bblUser, prop.Passwd ?? "") == false
            || bblUser.Username != prop.OldUsername
           )
            return Ok(new UpdateUsernameRes { HasWork = false });


        if (bblUser.LastLoginTime + TimeSpan.FromDays(7) > DateTime.UtcNow)
            return Ok(new UpdateUsernameRes {
                HasWork = false,
                WaitTimeForNextDayToUpdate = (bblUser.LastLoginTime + TimeSpan.FromDays(7) - DateTime.UtcNow).Days
            });

        log.AddResultAndTransform(db.Execute(@$"
UPDATE bbl_user 
SET username = @0, username_last_change = @1 
WHERE id = {userId}", prop.NewUsername!, DateTime.UtcNow));

        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/update/avatar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateAvatarRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateAvatar([FromBody] UpdateAvatarProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.AnyValidate() == EResult.Err)
            return Ok(new UpdateAvatarRes { PasswdFalse = true });
        
        var optionTokenInfo = log.AddResultAndTransform(LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        if (optionTokenInfo.IsSet() == false) return BadRequest();

        var userId = optionTokenInfo.Unwrap().UserId;

        byte[] imageBytes;
        try {
            var charArr = prop.ImageBase64.AsSpan(prop.ImageBase64!.IndexOf(',') + 1).ToArray();
            // TODO Write one Convert.FromBase64String With Span
            imageBytes = Convert.FromBase64CharArray(charArr, 0, charArr.Length);
        }
        catch (Exception e) {
            log.AddLogError(e.ToString());
            return BadRequest();
        }

        var bblUser = log.AddResultAndTransform(db
            .SingleOrDefault<BblUser>($"SELECT username, password, email, id FROM bbl_user WHERE id = {userId}"))
            .OkOrDefault();
        if (bblUser is null || bblUser.Password != ToPasswdHash(prop.Passwd ?? ""))
            return Ok(new UpdateAvatarRes { PasswdFalse = true });

        var imageMemoryStream = new MemoryStream(imageBytes);

        var image = Image.Load(imageMemoryStream);

        if (image.Height > 512 || image.Width > 512) {
            (int height, int width) newSize = image.Height > image.Width
                ? (512, image.Width / image.Height * 512)
                : (image.Height / image.Width * 512, 512);

            image.Mutate(x => x.Resize(newSize.height, newSize.width));
        }

        try {
            var filePath = $"{Env.AvatarPath}/{bblUser.Id}";
            image.SaveAsPng(filePath);
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception) {
            return GetInternalServerError();
        }
#endif

        return Ok(new UpdateAvatarRes());
    }

    [HttpPost("/api/profile/update/patreonemail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdatePatreonEmail([FromBody] UpdatePatreonEmailProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.AnyValidate() == EResult.Err)
            return Ok(ApiTypes.Work.False);

        var checkRes = log.AddResultAndTransform(Database.TableFn.BblUser.CheckPasswordGetId(db, prop.Username ?? "",
            Database.TableFn.BblUser.HashPasswd(prop.Passwd ?? "", Env.PasswdSeed))).OkOr(Option<long>.Empty);
        if (checkRes.IsSet() == false) return Ok(new ApiTypes.Work { HasWork = false });
        var userId = checkRes.Unwrap();

        var optionBblUser = log.AddResultAndTransform(Database.TableFn.BblUser.GetUserById(db, userId)).OkOr(Option<BblUser>.Empty);
        if (optionBblUser.IsSet() == false) return Ok(new ApiTypes.Work { HasWork = false });

        var bblUser = optionBblUser.Unwrap(); 

        var token = Guid.NewGuid();

        _patreoneMailToken.Add(token, (userId, prop.Email!));

        SendEmail.MainSendPatreonVerifyLinkToken(bblUser.Username!, bblUser.Email!, token);

        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpGet("/api/profile/accept/patreonemail/token/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AcceptPatreonEmail([FromRoute(Name = "token")] Guid token) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (token == Guid.Empty)
            return Ok(new ApiTypes.Work { HasWork = false });
        
        var response = _patreoneMailToken.Pop(token);

        if (response.IsSet() == false) {
            log.AddLogDebug($"Token Not Found: {token}");
            return Ok(new ApiTypes.Work { HasWork = false });
        }

        var userIdAndEmail = response.Unwrap();

        var err = Database.TableFn.BblUser.SetPatreonEmail(db, userIdAndEmail.UserId, userIdAndEmail.Email!);
        if (err == EResult.Err) {
            log.AddLogError(err.Err());
            return Ok(new ApiTypes.Work { HasWork = false });
        }
        err = Database.TableFn.BblUser.SetAcceptPatreonEmail(db, userIdAndEmail.UserId);
        if (err == EResult.Err) {
            log.AddLogError(err.Err());
            return Ok(new ApiTypes.Work { HasWork = false });
        }
        
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/drop-account/sendMail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateDropAccountTokenRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateDropAccountToken([FromBody] ApiTypes.Api2GroundNoHeader<CreateDropAccountTokenProp> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.ValuesAreGood() == false) 
            return Ok(CreateDropAccountTokenRes.HasElseError());
        
        var optionTokenInfo = log.AddResultAndTransform(LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        if (optionTokenInfo.IsSet() == false) return BadRequest();

        var userId = optionTokenInfo.Unwrap().UserId;
        
        var optionBblUser = log.AddResultAndTransform(Database.TableFn.BblUser.GetUserById(db, userId)).OkOr(Option<BblUser>.Empty);
        if (optionBblUser.IsSet() == false)
            return Ok(CreateDropAccountTokenRes.HasElseError());

        var bblUser = optionBblUser.Unwrap();
        if (Database.TableFn.BblUser.PasswordEqual(bblUser, prop.Body!.Password ?? "") == false) 
            return Ok(CreateDropAccountTokenRes.PasswordIsFalse());
        
        
        var deleteAccToken = Guid.NewGuid(); 
        _deleteAccMailToken.Add(deleteAccToken, (userId, bblUser.Email??""));
        Utils.SendEmail.MainSendDropAccountVerifyLinkToken(bblUser.Username??"", bblUser.Email??"", deleteAccToken);

        return Ok(CreateDropAccountTokenRes.NoError());
    }

    [HttpGet("/api/profile/drop-account/token/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DropAccountWithToken([FromRoute(Name = "token")] Guid token) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        _deleteAccMailToken.CleanDeadTokens();
        
        var optionTokenInfo = log.AddResultAndTransform(LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        var deleteAccTokenResponse = _deleteAccMailToken.Pop(token: token);
        if (optionTokenInfo.IsSet() == false || deleteAccTokenResponse.IsSet() == false)
            return BadRequest();

        var userId = deleteAccTokenResponse.Unwrap().UserId;

        ResultErr<string> deleteAccountResponse = Bbl.DeleteAccount(db, userId);
        if (deleteAccountResponse == EResult.Err)
            log.AddLogError(deleteAccountResponse.Err());
        this.RemoveCookieByEName(ECookie.LoginCookie);
        return Ok(ApiTypes.Work.True);
    }


    [HttpGet("/api/profile/top-play-by-marks-length/user-id/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlaysMarksLength))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileTopPlaysByMarksLength([FromRoute] long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();

        if (userId < 0) return BadRequest("UserId < 0");

        var result = log.AddResultAndTransform(OsuDroid.Database.TableFn.BblScore.CountMarkPlaysByUserId(db, userId));
        if (result == EResult.Err)
            return GetInternalServerError();

        return Ok(PlaysMarksLength.Factory(result.Ok()));
    }

    [HttpGet("/api/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Plays))]
    public IActionResult WebProfileTopPlaysByMark(
        [FromRoute] long userId, [FromRoute] string markString, [FromRoute] int page) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();

        if (long.IsNegative(userId))
            return BadRequest("userid Is Negative");
        if (string.IsNullOrEmpty(markString))
            return BadRequest("markString Is Null Or Empty");
        if (int.IsNegative(page))
            return BadRequest("page Is Negative");
        
        if (!Enum.TryParse<BblScore.EMark>(markString, out BblScore.EMark mark))
            return BadRequest("markString Case Not Exist");

        var fetchResult = log.AddResultAndTransform(OsuDroid.Database.TableFn.BblScore.GetTopScoreFromUserIdFilterMark(db, userId, page, 50, mark));
        if (fetchResult == EResult.Err)
            return GetInternalServerError();

        var scores = fetchResult.Ok();
        return Ok(new Plays() { Found = true, Scores = scores });
    }
    
    
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class CreateDropAccountTokenProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString {
        public string? Password { get; set; }
        
        public bool ValuesAreGood() => !string.IsNullOrEmpty(Password);

        public string ToSingleString() => Password??"";
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class CreateDropAccountTokenRes {
        public bool Work { get; set; }
        public bool PasswordFalse { get; set; }
        public bool CookieDead { get; set; }
        public bool ElseError { get; set; }

        public static CreateDropAccountTokenRes NoError() => new() { Work = true };
        public static CreateDropAccountTokenRes PasswordIsFalse() => new() { Work = false, PasswordFalse = true };
        public static CreateDropAccountTokenRes CookieIsDead() => new() { Work = false, CookieDead = true };
        public static CreateDropAccountTokenRes HasElseError() => new() { Work = false, ElseError = true };
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdatePatreonEmailProp : ValidateAll, IValidateEmail, IValidateUsername, IValidatePasswd {
        public string? Email { get; set; }
        public string? Passwd { get; set; }
        public string? Username { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UserRankTimeLine {
        public long UserId { get; set; }
        public IReadOnlyList<RankTimeLineValue>? List { get; set; }

        public class RankTimeLineValue {
            public DateTime Date { get; set; }
            public long Score { get; set; }
            public long Rank { get; set; }
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdateAvatarProp : ValidateAll, IValidatePasswd {
        public string? ImageBase64 { get; set; }
        public string? Passwd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdateAvatarRes {
        public bool PasswdFalse { get; set; }
        public bool ImageToBig { get; set; }
        public bool IsNotAImage { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdateUsernameRes {
        public bool HasWork { get; set; }
        public int WaitTimeForNextDayToUpdate { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdateEmailProp : ValidateAll, IValidateOldEmail, IValidateNewEmail, IValidatePasswd {
        public string? NewEmail { get; set; }
        public string? OldEmail { get; set; }
        public string? Passwd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdateUsernameProp : ValidateAll, IValidateOldUsername, IValidateNewUsername, IValidatePasswd {
        public string? NewUsername { get; set; }
        public string? OldUsername { get; set; }
        public string? Passwd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdatePasswdProp : ValidateAll, IValidateOldPasswd, IValidateNewPasswd {
        public string? NewPasswd { get; set; }
        public string? OldPasswd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class Plays {
        public bool Found { get; set; }
        public IReadOnlyList<BblScore>? Scores { get; set; }
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class PlaysMarksLength {
        public long PlaysXSS { get; set; }
        public long PlaysSS { get; set; }
        public long PlaysXS { get; set; }
        public long PlaysS { get; set; }
        public long PlaysA { get; set; }
        public long PlaysB { get; set; }
        public long PlaysC { get; set; }
        public long PlaysD { get; set; }
        public long PlaysAll { get; set; }

        public static PlaysMarksLength Factory(Dictionary<BblScore.EMark, long> dictionary) {
            return new PlaysMarksLength() {
                PlaysXSS = dictionary.ContainsKey(BblScore.EMark.XSS)? dictionary[BblScore.EMark.XSS]: 0,
                PlaysSS = dictionary.ContainsKey(BblScore.EMark.SS)? dictionary[BblScore.EMark.SS]: 0,
                PlaysXS = dictionary.ContainsKey(BblScore.EMark.XS)? dictionary[BblScore.EMark.XS]: 0,
                PlaysS = dictionary.ContainsKey(BblScore.EMark.S)? dictionary[BblScore.EMark.S]: 0,
                PlaysA = dictionary.ContainsKey(BblScore.EMark.A)? dictionary[BblScore.EMark.A]: 0,
                PlaysB = dictionary.ContainsKey(BblScore.EMark.B)? dictionary[BblScore.EMark.B]: 0,
                PlaysC = dictionary.ContainsKey(BblScore.EMark.C)? dictionary[BblScore.EMark.C]: 0,
                PlaysD = dictionary.ContainsKey(BblScore.EMark.D)? dictionary[BblScore.EMark.D]: 0,
                PlaysAll = dictionary.Select(x => x.Value).Sum()
            };
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class ProfileStats {
        public long Id { get; set; }
        public string? Username { get; set; }
        public bool Found { get; set; }
        public string? Region { get; set; }
        public bool Active { get; set; }
        public bool Supporter { get; set; }
        public DateTime RegistTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public long GlobalRanking { get; set; }
        public long CountryRanking { get; set; }

        public long OverallScore { get; set; }
        public long OverallAccuracy { get; set; }
        public long OverallCombo { get; set; }
        public long OverallXss { get; set; }
        public long OverallSs { get; set; }
        public long OverallXs { get; set; }
        public long OverallS { get; set; }
        public long OverallA { get; set; }
        public long OverallB { get; set; }
        public long OverallC { get; set; }
        public long OverallD { get; set; }
        public long OverallHits { get; set; }
        public long OverallPerfect { get; set; }
        public long Overall300 { get; set; }
        public long Overall100 { get; set; }
        public long Overall50 { get; set; }
        public long OverallGeki { get; set; }
        public long OverallKatu { get; set; }
        public long OverallMiss { get; set; }
        public long OverallPlaycount { get; set; }
    }
}