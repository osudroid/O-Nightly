using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Authentication.Domain;

/// <summary>
/// Provides functionality for user authentication, including retrieving user authentication contexts
/// by username, user ID, or email.
/// </summary>
public class AuthenticationProvider: IAuthenticationProvider {
    private readonly IQueryUserInfo _queryUserInfo;

    /// <summary>
    /// Gets the password provider for the first generation of passwords.
    /// </summary>
    public IPasswordProvider PasswordGen1Provider { get; init; } = new Password.PasswordGen1Provider();
    
    /// <summary>
    /// Gets the password provider for the second generation of passwords.
    /// </summary>
    public IPasswordProvider PasswordGen2Provider { get; init; } = new Password.PasswordGen2Provider();
    
    public AuthenticationProvider(IQueryUserInfo queryUserInfo) {
        _queryUserInfo = queryUserInfo;
    }

    /// <summary>
    /// Retrieves the user authentication context by username.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>
    /// A <see cref="ResultOk{T}"/> containing an <see cref="Option{T}"/> with the user authentication context
    /// if found, or an empty result if not found.
    /// </returns>
    public async Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUsername(string username) {
        return (await _queryUserInfo.GetByUsernameAsync(username))
            .Map(x => x.Map(x => (IUserAuthContext) UserAuthContext.FromUserInfo(x, _queryUserInfo)));
    }

    /// <summary>
    /// Retrieves the user authentication context by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>
    /// A <see cref="ResultOk{T}"/> containing an <see cref="Option{T}"/> with the user authentication context
    /// if found, or an empty result if not found.
    /// </returns>
    public async Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUserId(long userId) {
        return (await _queryUserInfo.GetByUserIdAsync(userId))
            .Map(x => x.Map(x => (IUserAuthContext) UserAuthContext.FromUserInfo(x, _queryUserInfo)));
    }
    
    /// <summary>
    /// Retrieves the user authentication context by email.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>
    /// A <see cref="ResultOk{T}"/> containing an <see cref="Option{T}"/> with the user authentication context
    /// if found, or an empty result if not found.
    /// </returns>
    public async Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByEmail(string email) {
        return (await _queryUserInfo.GetByEmailAsync(email))
            .Map(x => x.Map(x => (IUserAuthContext) UserAuthContext.FromUserInfo(x, _queryUserInfo)));
    }
}