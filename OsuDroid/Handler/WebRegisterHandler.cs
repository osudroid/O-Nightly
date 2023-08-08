using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler; 

public class WebRegisterHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<WebRegisterDto>, OptionHandlerOutput<ViewWebLogin>>{
    
    public async ValueTask<Result<OptionHandlerOutput<ViewWebLogin>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<WebRegisterDto> request) {
        
        var now = DateTime.UtcNow;
        var data = request.Post;
        var db = dbWrapper.Db;
        var controller = request.Controller;
        
        var tokenResult = await WebLoginMathResultManager.GetWebLoginTokenAsync(db, data.MathToken);
        if (tokenResult == EResult.Err) {
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Err(tokenResult.ToString()!);
        }

        if (tokenResult.Ok().IsNotSet() 
            || tokenResult.Ok().Unwrap().MathResult != data.MathRes
           ) {
            
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Ok(OptionHandlerOutput<ViewWebLogin>.With(new ViewWebLogin() {
                Work = false,
                EmailFalse = false,
                UsernameFalse = false,
                UserOrPasswdOrMathIsFalse = true
            }));
        }

        var findResult = await QueryUserInfo.GetEmailAndUsernameByEmailAndUsername(db, data.Email, data.Username);

        if (findResult == EResult.Err)
            return findResult.ChangeOkType<OptionHandlerOutput<ViewWebLogin>>();

        var find = Option<Entities.UserInfo>.NullSplit(findResult.Ok().FirstOrDefault());

        if (find.IsSet()) {
            if (find.Unwrap().Username == data.Username)
                return Result<OptionHandlerOutput<ViewWebLogin>, string>
                    .Ok(OptionHandlerOutput<ViewWebLogin>.With(new ViewWebLogin { UsernameFalse = true }));
            if (find.Unwrap().Email == data.Email)
                return Result<OptionHandlerOutput<ViewWebLogin>, string>
                    .Ok(OptionHandlerOutput<ViewWebLogin>
                        .With(new ViewWebLogin { EmailFalse = true }));
        }
        
        
        var optionIp = request.Controller.GetIpAddress().OkOrDefault();
        if (optionIp.IsSet() == false) {
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Err(TraceMsg.WithMessage("ip not found"));
        }

        var ip = optionIp.Unwrap();

        
        var optionCountry = CountryInfo.FindByName((IpInfo.Country(ip)?.Country.Name) ?? "");
        var newUser = new Entities.UserInfo {
            Active = true,
            Banned = false,
            DeviceId = "",
            Email = (data.Email ?? "").ToLower(),
            Password = PasswordHash.HashWithBCryptPassword(data.Passwd).Ok(),
            Username = data.Username,
            Region = optionCountry.IsSet() ? optionCountry.Unwrap().NameShort : "",
            LatestIp = ip.ToString(),
            RegisterTime = DateTime.UtcNow,
            RestrictMode = false,
            LastLoginTime = DateTime.UtcNow,
            UsernameLastChange = DateTime.UtcNow
        };
        var resultErr = await QueryUserInfo.InsertAsync(db, newUser);
        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<OptionHandlerOutput<ViewWebLogin>>();

        var userIdOpt = (await QueryUserInfo.GetUserIdByUsernameAsync(db, newUser.Username))
                        .Map(x => x.IsNotSet() ? Option<long>.Empty : Option<long>.With(x.Unwrap().UserId))
                        .OkOr(Option<long>.Empty);

        if (userIdOpt.IsNotSet())
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Err(TraceMsg.WithMessage("userIdOpt Is Not Set"));

        resultErr = await QueryUserStats.InsertAsync(db, new() { UserId = userIdOpt.Unwrap() });

        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<OptionHandlerOutput<ViewWebLogin>>();

        return Result<OptionHandlerOutput<ViewWebLogin>, string>
            .Ok(OptionHandlerOutput<ViewWebLogin>.With(new ViewWebLogin { Work = true }));
    }
}