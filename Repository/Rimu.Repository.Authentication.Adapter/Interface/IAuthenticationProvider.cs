using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IAuthenticationProvider {
    /// <summary>
    /// Retrieves the user authentication context by username.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>
    /// A <see cref="ResultOk{T}"/> containing an <see cref="Option{T}"/> with the user authentication context
    /// if found, or an empty result if not found.
    /// </returns>
    public Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUsername(string username);
    /// <summary>
    /// Retrieves the user authentication context by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>
    /// A <see cref="ResultOk{T}"/> containing an <see cref="Option{T}"/> with the user authentication context
    /// if found, or an empty result if not found.
    /// </returns>
    public Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUserId(long userId);
    /// <summary>
    /// Retrieves the user authentication context by email.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>
    /// A <see cref="ResultOk{T}"/> containing an <see cref="Option{T}"/> with the user authentication context
    /// if found, or an empty result if not found.
    /// </returns>
    public Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByEmail(string email);
    
    /// <summary>
    /// Gets the password provider for the first generation of passwords.
    /// </summary>
    IPasswordProvider PasswordGen1Provider { get; }

    /// <summary>
    /// Gets the password provider for the second generation of passwords.
    /// </summary>
    IPasswordProvider PasswordGen2Provider { get; }
}