using LamLibAllOver.ErrorHandling;
using NLog;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Authentication.Domain.Password;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Authentication.Domain;

/// <summary>
/// Represents the user authentication context, providing functionality for verifying passwords,
/// updating passwords, and managing user rules.
/// </summary>
public class UserAuthContext: IUserAuthContext {
    private readonly IPasswordProvider _passwordGen1Provider = new PasswordGen1Provider();
    private readonly IPasswordProvider _passwordGen2Provider = new PasswordGen2Provider();
    private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IQueryUserInfo _queryUserInfo; 

    /// <summary>
    /// Gets a value indicating whether the user was found and authorized.
    /// </summary>
    public bool FoundAndAuthorized { get; }
    /// <summary>
    /// Gets IUserAuthDataContext.
    /// </summary>
    public Option<IUserAuthDataContext> UserDataContext { get; private set; }
    /// <summary>
    /// Gets the user rule associated with the user.
    /// </summary>
    public IUserRule Rule { get; private init; }

    private UserAuthContext(bool foundAndAuthorized, Option<IUserAuthDataContext> userDataContext, IQueryUserInfo queryUserInfo, UserRule rule) {
        FoundAndAuthorized = foundAndAuthorized;
        UserDataContext = userDataContext;
        _queryUserInfo = queryUserInfo;
        Rule = rule;
    }
    
    /// <summary>
    /// Creates a new <see cref="UserAuthContext"/> from the given user information.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <param name="queryUserInfo">The query interface for user information.</param>
    /// <returns>A new instance of <see cref="UserAuthContext"/>.</returns>
    public static UserAuthContext FromUserInfo(UserInfo userInfo, IQueryUserInfo queryUserInfo) {
        return new UserAuthContext(
            userInfo.Active && !userInfo.Banned,
            Option<IUserAuthDataContext>.With(UserAuthDataContext.FromUserInfo(userInfo)),
            queryUserInfo,
            UserAuthContext.UserRule.FromUserInfo(userInfo)
        );
    }
    
    /// <summary>
    /// Verifies if the given password matches the stored password.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <returns>A <see cref="ResultOk{T}"/> indicating whether the password is correct.</returns>
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

    /// <summary>
    /// Checks if the given password hash matches the stored password hash for generation 1.
    /// </summary>
    /// <param name="passwordHash">The password hash to compare.</param>
    /// <returns>True if the hashes match; otherwise, false.</returns>
    public bool PasswordGen1EqualHash(string passwordHash) {
        return this.UserDataContext.Unwrap().PasswordGen1 == passwordHash;
    }

    /// <summary>
    /// Validates the password and sets a generation 2 password hash if it generation 1.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="ResultOk{T}"/> indicating success or failure.</returns>
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

    /// <summary>
    /// Updates the user's password with new generation 1 and generation 2 hashes.
    /// </summary>
    /// <param name="password">The new password.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="ResultNone"/> indicating success or failure.</returns>
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
    
    /// <summary>
    /// Represents the user rule, defining various permissions and restrictions for the user.
    /// </summary>
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