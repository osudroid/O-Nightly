using LamLibAllOver.ErrorHandling;
using NLog;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Authentication.Domain.Password;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Authentication.Domain;

public class UserAuthContext: IUserAuthContext {
    private readonly IPasswordProvider _passwordGen1Provider = new PasswordGen1Provider();
    private readonly IPasswordProvider _passwordGen2Provider = new PasswordGen2Provider();
    private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IQueryUserInfo _queryUserInfo; 
    public bool FoundAndAuthorized { get; }
    public Option<IUserAuthDataContext> UserDataContext { get; private set; }
    public IUserRule Rule { get; private init; }

    private UserAuthContext(bool foundAndAuthorized, Option<IUserAuthDataContext> userDataContext, IQueryUserInfo queryUserInfo, UserRule rule) {
        FoundAndAuthorized = foundAndAuthorized;
        UserDataContext = userDataContext;
        _queryUserInfo = queryUserInfo;
        Rule = rule;
    }
    
    public static UserAuthContext FromUserInfo(UserInfo userInfo, IQueryUserInfo queryUserInfo) {
        return new UserAuthContext(
            userInfo.Active && !userInfo.Banned,
            Option<IUserAuthDataContext>.With(UserAuthDataContext.FromUserInfo(userInfo)),
            queryUserInfo,
            UserAuthContext.UserRule.FromUserInfo(userInfo)
        );
    }
    
    public ResultOk<bool> IsPassword(string password) {
        if (UserDataContext.IsNotSet()) {
            _logger.Error($"UserDataContext {password} is not set");
        }

        var user = UserDataContext.Unwrap();
        if (user.PasswordGen2.IsSet()) {
            return ResultOk<bool>.Ok(_passwordGen2Provider.VerifyPassword(password, user.PasswordGen2.Unwrap()));
        }

        return ResultOk<bool>.Ok(_passwordGen1Provider.VerifyPassword(password, user.PasswordGen1));
    }

    public bool PasswordGen1EqualHash(string passwordHash) {
        return this.UserDataContext.Unwrap().PasswordGen1 == passwordHash;
    }

    public Task<ResultOk<bool>> IsPasswordValidAndSetGen2IfNotExistAsync(string password) {
        return IsPassword(password).AndThenAsync(async x => {
            if (!x) {
                return ResultOk<bool>.Ok(false);
            }

            if (UserDataContext.Unwrap().PasswordGen2.IsNotSet()) {
                return await UpdatePasswordAsync(password) == EResult.Ok
                    ? ResultOk<bool>.Ok(true)
                    : ResultOk<bool>.Err();
            }
            
            return ResultOk<bool>.Ok(true);
        });
    }

    public async Task<ResultNone> UpdatePasswordAsync(string password) {
        var passwordGen1Hash = _passwordGen1Provider.HashPassword(password);
        var passwordGen2Hash = _passwordGen1Provider.HashPassword(password);


        if (this.UserDataContext.IsNotSet()) {
            _logger.Error("UserDataContext is not set");
            return ResultNone.Err;
        }

        return await _queryUserInfo.UpdatePasswordAsync(
            this.UserDataContext.Unwrap().UserId,
            passwordGen1Hash,
            passwordGen2Hash
        );
    }
    
    public struct UserRule: IUserRule {
        public required bool IsRestrict { get; init; }
        public required bool IsBanned { get; init; }
        public required bool IsArchived { get; init; }
        
        public required bool Login  { get; init; }
        public required bool Multiplayer  { get; init; }
        public required bool ScoreSubmission  { get; init; }
        public required bool GlobalRanking  { get; init; }
        public required bool BeatmapRanking  { get; init; }
        public required bool ProfilePageAccess  { get; init; }

        public static UserRule FromUserInfo(UserInfo userInfo) {
            var restrict = userInfo.RestrictMode;
            var banned = userInfo.Banned;
            var archived = userInfo.Archived;
            
            return new UserRule() {
                IsRestrict = restrict,
                IsBanned = banned,
                IsArchived = archived,
                Login = !(restrict || banned || archived),
                Multiplayer = !(restrict || banned || archived),
                ScoreSubmission = !(restrict || banned || archived),
                GlobalRanking = !(restrict || banned || archived),
                BeatmapRanking = !(restrict || banned),
                ProfilePageAccess = !(restrict || banned),
            };
        }
    }
}