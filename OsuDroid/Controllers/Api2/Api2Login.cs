using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib.TokenHandler;
using OsuDroidLib.Database.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2;

public class Api2Login : ControllerExtensions {
    [HttpPost("/api2/token-create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateApi2TokenResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateApi2Token([FromBody] CreateApi2TokenProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var user = db.SingleOrDefault<BblUser>("SELECT id, username, password FROM bbl_user WHERE username = lower(@0)",
            prop.Username ?? "").OkOrDefault();

        if (user is null)
            return Ok(new CreateApi2TokenResult {
                Token = Guid.Empty,
                PasswdFalse = false,
                UsernameFalse = true
            });

        if (user.Password != ToPasswdHash(prop.Passwd ?? string.Empty))
            return Ok(new CreateApi2TokenResult {
                Token = Guid.Empty,
                PasswdFalse = true,
                UsernameFalse = false
            });

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User);
        return Ok(new CreateApi2TokenResult {
            Token = tokenHandler.Insert(db, user.Id),
            PasswdFalse = false,
            UsernameFalse = false
        });
    }

    [HttpPost("/api2/token-refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult RefreshApi2Token([FromBody] SimpleTokenProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User);
        if (tokenHandler.TokenExist(db, prop.Token) == false)
            Ok(new ApiTypes.Work { HasWork = false });
        tokenHandler.Refresh(db, prop.Token);
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api2/token-remove")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult RemoveApi2Token([FromBody] SimpleTokenProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User);
        tokenHandler.RemoveToken(db, prop.Token);
        return Ok(new ApiTypes.Work { HasWork = true });
    }

    [HttpPost("/api2/token-user-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<long>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetTokenUserId([FromBody] SimpleTokenProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User);
        var resp = tokenHandler.GetTokenInfo(db, prop.Token);
        return resp == EResponse.Err
            ? Ok(new ApiTypes.ExistOrFoundInfo<long> { Value = -1, ExistOrFound = false })
            : Ok(new ApiTypes.ExistOrFoundInfo<long> { Value = resp.Ok().UserId, ExistOrFound = true });
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CreateApi2TokenProp {
        public string? Username { get; set; }
        public string? Passwd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CreateApi2TokenResult {
        public required Guid Token { get; set; }
        public required bool UsernameFalse { get; set; }
        public required bool PasswdFalse { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SimpleTokenProp {
        public Guid Token { get; set; }
    }
}