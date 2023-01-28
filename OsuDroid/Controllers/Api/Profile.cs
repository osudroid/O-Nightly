using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using OsuDroid.Database.TableFn;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.Validate;
using OsuDroid.Utils;
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
        BblUserAndBblUserStats? userAndStats;
        var sql = new Sql(
            "SELECT * FROM public.bbl_user JOIN bbl_user_stats bus on bus.uid = bbl_user.id WHERE id = @0", userId);
        using var db = DbBuilder.BuildPostSqlAndOpen();


        userAndStats = db.SingleOrDefault<BblUserAndBblUserStats>(sql).OkOrDefault();

        if (userAndStats is null)
            return Ok(new ProfileStats { Found = false });

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
        var userRank = db.Fetch<BblUser.UserRank>(sqlRank).OkOrDefault()?.FirstOrDefault();

        if (userRank is null)
            return GetInternalServerError();

        BblPatron? bblPatron = null;
        if (string.IsNullOrEmpty(userAndStats.Email) == false) {
            var response = Database.TableFn.BblUser.GetBblPatron(new BblUser { PatronEmail = userAndStats.Email }, db);
            bblPatron = response == EResponse.Ok ? response.Ok() : null;
        }

        return Ok(new ProfileStats {
            Username = userAndStats.Username,
            Id = userAndStats.Id,
            Found = true,
            Playcount = userAndStats.Playcount,
            Accuracy = userAndStats.OverallAccuracy,
            Region = userAndStats.Region,
            Active = userAndStats.Active,
            Supporter = bblPatron is null ? false : bblPatron.ActiveSupporter,
            GlobalRanking = userRank.globalRank,
            CountryRanking = userRank.CountryRank,
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
        if (userId < 0)
            return BadRequest();
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var res = BblGlobalRankingTimeline
            .BuildTimeLine(db, userId, DateTime.UtcNow - TimeSpan.FromDays(90))
            .Select(x => new UserRankTimeLine.RankTimeLineValue {
                Date = x.Date,
                Score = x.Score,
                Rank = x.GlobalRanking
            }).ToList();

        return Ok(new UserRankTimeLine {
            UserId = userId,
            List = res
        });
    }

    [HttpGet("/api/profile/topplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileTopPlays([FromRoute(Name = "id")] long userId) {
        var sql = new Sql(@$"
SELECT * FROM bbl_score 
WHERE uid = {userId}
ORDER BY bbl_score.score DESC 
LIMIT 50;
");

        using var db = DbBuilder.BuildPostSqlAndOpen();
        return Ok(new Plays {
            Found = true,
            Scores = db.Fetch<BblScore>(sql).OkOrDefault() ?? new List<BblScore>(0)
        });
    }

    [HttpGet("/api/profile/recentplays/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Plays))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebProfileTopRecent([FromRoute(Name = "id")] long userId) {
        var sql = new Sql(@$"
SELECT * FROM bbl_score 
WHERE uid = {userId}
ORDER BY bbl_score.id DESC 
LIMIT 50;
");

        using var db = DbBuilder.BuildPostSqlAndOpen();
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
        if (prop.AnyValidate() == EResponse.Err)
            return Ok(new ApiTypes.Work { HasWork = false });

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenInfoResp = LoginTokenInfo(db);
        if (tokenInfoResp == EResponse.Err) return Ok(ApiTypes.Work.False);

        var userId = tokenInfoResp.Ok().UserId;

        var bblUser = db
            .SingleOrDefault<BblUser>($"SELECT email, password FROM bbl_user WHERE id = {userId}")
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
        catch {
            /* ignored */
        }

        db.Execute(@$"UPDATE bbl_user SET email = @0 WHERE id = {userId}", prop.NewEmail!);
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/update/passwd")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdatePasswd([FromBody] UpdatePasswdProp prop) {
        if (prop.AnyValidate() == EResponse.Err)
            return Ok(new ApiTypes.Work { HasWork = false });

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenInfoResp = LoginTokenInfo(db);
        if (tokenInfoResp == EResponse.Err) return Ok(ApiTypes.Work.False);

        var userId = tokenInfoResp.Ok().UserId;

        var bblUser = db
            .SingleOrDefault<BblUser>($"SELECT password FROM bbl_user WHERE id = {userId}")
            .OkOrDefault();
        if (bblUser is null)
            return Ok(new ApiTypes.Work { HasWork = false });

        var newPasswdHash = MD5.Hash(prop.NewPasswd + Env.PasswdSeed).ToLower();
        var oldPasswdHash = MD5.Hash(prop.OldPasswd + Env.PasswdSeed).ToLower();
        if (bblUser.Password != oldPasswdHash)
            return Ok(new ApiTypes.Work { HasWork = false });

        db.Execute(@$"UPDATE bbl_user SET password = @0 WHERE id = {userId}", newPasswdHash!);
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/update/username")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateUsernameRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateUsername([FromBody] UpdateUsernameProp prop) {
        prop.NewUsername = prop.NewUsername?.Trim();
        if (prop.AnyValidate() == EResponse.Err)
            return Ok(new UpdateUsernameRes { HasWork = false });

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenInfoResp = LoginTokenInfo(db);
        if (tokenInfoResp == EResponse.Err) return Ok(new UpdateUsernameRes { HasWork = false });

        var userId = tokenInfoResp.Ok().UserId;

        var bblUser =
            db.SingleOrDefault<BblUser>(
                    $"SELECT username, password, username_last_change FROM bbl_user WHERE id = {userId}")
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

        db.Execute(@$"
UPDATE bbl_user 
SET username = @0, username_last_change = @1 
WHERE id = {userId}", prop.NewUsername!, DateTime.UtcNow);

        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/update/avatar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateAvatarRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateAvatar([FromBody] UpdateAvatarProp prop) {
        if (prop.AnyValidate() == EResponse.Err)
            return Ok(new UpdateAvatarRes { PasswdFalse = true });

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenInfoResp = LoginTokenInfo(db);
        if (tokenInfoResp == EResponse.Err) return BadRequest();

        var userId = tokenInfoResp.Ok().UserId;

        byte[] imageBytes;
        try {
            var charArr = prop.ImageBase64.AsSpan(prop.ImageBase64!.IndexOf(',') + 1).ToArray();
            // TODO Write one Convert.FromBase64String With Span
            imageBytes = Convert.FromBase64CharArray(charArr, 0, charArr.Length);
        }
        catch (Exception) {
            return BadRequest();
        }

        var bblUser = db
            .SingleOrDefault<BblUser>($"SELECT username, password, email, id FROM bbl_user WHERE id = {userId}")
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
        if (prop.AnyValidate() == EResponse.Err)
            return Ok(ApiTypes.Work.False);

        using var db = DbBuilder.BuildPostSqlAndOpen();

        var checkRes = Database.TableFn.BblUser.CheckPasswordGetId(db, prop.Username ?? "",
            Database.TableFn.BblUser.HashPasswd(prop.Passwd ?? "", Env.PasswdSeed));
        if (checkRes == EResponse.Err) return Ok(new ApiTypes.Work { HasWork = false });
        var userId = checkRes.Ok();

        var bblUser = Database.TableFn.BblUser.GetUserById(db, userId);
        if (bblUser is null) return Ok(new ApiTypes.Work { HasWork = false });


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
        if (token == Guid.Empty)
            return Ok(new ApiTypes.Work { HasWork = false });

        var response = _patreoneMailToken.Pop(token);

        if (response == EResponse.Err)
            return Ok(new ApiTypes.Work { HasWork = false });

        var userIdAndEmail = response.Ok();

        using var db = DbBuilder.BuildPostSqlAndOpen();

        Database.TableFn.BblUser.SetPatreonEmail(db, userIdAndEmail.UserId, userIdAndEmail.Email!);
        Database.TableFn.BblUser.SetAcceptPatreonEmail(db, userIdAndEmail.UserId);
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api/profile/drop-account/sendMail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateDropAccountTokenRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateDropAccountToken([FromBody] ApiTypes.Api2GroundNoHeader<CreateDropAccountTokenProp> prop) {
        if (prop.ValuesAreGood() == false) 
            return Ok(CreateDropAccountTokenRes.HasElseError());
        
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenInfoRes = LoginTokenInfo(db);
        if (tokenInfoRes == EResponse.Err)
            return Ok(CreateDropAccountTokenRes.CookieIsDead());
        
        var userId = tokenInfoRes.Ok().UserId;
        var bblUser = Database.TableFn.BblUser.GetUserById(db, userId);
        if (bblUser is null)
            return Ok(CreateDropAccountTokenRes.HasElseError());
        
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
        _deleteAccMailToken.CleanDeadTokens();
        using var db = DbBuilder.BuildPostSqlAndOpen();
        
        var tokenInfoRes = LoginTokenInfo(db);
        var deleteAccTokenResponse = _deleteAccMailToken.Pop(token: token);
        if (tokenInfoRes == EResponse.Err || deleteAccTokenResponse == EResponse.Err)
            return BadRequest();

        var userId = deleteAccTokenResponse.Ok().UserId;

        var deleteAccountResponse = Bbl.DeleteAccount(db, userId);
        
        this.RemoveCookieByEName(ECookie.LoginCookie);
        return Ok(ApiTypes.Work.True);
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
    public sealed class ProfileStats {
        public long Id { get; set; }
        public string? Username { get; set; }
        public bool Found { get; set; }
        public long Accuracy { get; set; }
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
        public long Playcount { get; set; }
    }
}