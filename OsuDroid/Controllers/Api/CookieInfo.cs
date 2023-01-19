using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api;

public sealed class CookieInfo : ControllerExtensions {
    [HttpGet("/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<UserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ExistOrFoundInfo<UserInfo>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUserInfoByCookie() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var response = LoginTokenInfo(db);
        if (response == EResponse.Err) return Ok(ApiTypes.ExistOrFoundInfo<UserInfo>.NotExist());

        var user = db.SingleOrDefaultById<BblUser>(response.Ok().UserId).OkOrDefault();
        if (user is null)
            return Ok(new ApiTypes.ExistOrFoundInfo<UserInfo> { ExistOrFound = false, Value = null });

        return Ok(new ApiTypes.ExistOrFoundInfo<UserInfo> {
            ExistOrFound = true,
            Value = new UserInfo {
                Active = user.Active,
                Banned = user.Banned,
                Email = user.Email,
                Id = user.Id,
                Region = user.Region,
                Username = user.Username,
                RegistTime = user.RegistTime,
                RestrictMode = user.RestrictMode
            }
        });
    }


    public sealed class UserInfo {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public DateTime RegistTime { get; set; }
        public string? Region { get; set; }
        public bool Active { get; set; }
        public bool Supporter { get; set; }
        public bool Banned { get; set; }
        public bool RestrictMode { get; set; }
    }
}