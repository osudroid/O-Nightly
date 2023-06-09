using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;

namespace OsuDroid.Controllers.Api;

public sealed class CookieInfo : ControllerExtensions {
    [HttpGet("/api/user-info-by-cookie")]
    [PrivilegeRoute(route: "/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<UserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ExistOrFoundInfo<UserInfo>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfoByCookie() {
        await using var start = await GetStartAsync();
        var (db, log) = start.Unpack();
        
        await log.AddLogDebugStartAsync();
        await log.AddLogOkAsync("Start");
        try {
            var optionToken = log.AddResultAndTransform(LoginTokenInfo(db.Connection!)).OkOr(Option<TokenInfo>.Empty);
         
            if (optionToken.IsSet() == false) 
                return Ok(ApiTypes.ExistOrFoundInfo<UserInfo>.NotExist());

            var token = optionToken.Unwrap();
            
            var optionUser = log.AddResultAndTransform(db.SingleOrDefaultById<Entities.UserInfo>(token.UserId))
                                .Map(x => Option<Entities.UserInfo>.NullSplit(x))
                                .OkOr(Option<Entities.UserInfo>.Empty);
            if (optionUser.IsSet() == false)
                return Ok(new ApiTypes.ExistOrFoundInfo<UserInfo> { ExistOrFound = false, Value = null });

            var user = optionUser.Unwrap();
            return Ok(new ApiTypes.ExistOrFoundInfo<UserInfo> {
                ExistOrFound = true,
                Value = new UserInfo {
                    Active = user.Active,
                    Banned = user.Banned,
                    Email = user.Email,
                    Id = user.UserId,
                    Region = user.Region,
                    Username = user.Username,
                    RegistTime = user.RegisterTime,
                    RestrictMode = user.RestrictMode
                }
            });
            
            await db.CommitAsync();
        }
        catch (Exception e) {
            WriteLine(e);
            await log.AddLogErrorAsync("Exception", Option<string>.With(e.ToString()));
            await db.RollbackAsync();
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
public class A: IAsyncDisposable {
    public async ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}