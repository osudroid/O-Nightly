using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Authentication.Adapter.Interface;

/// <summary>
/// Represents the user authentication context, providing functionality for verifying passwords,
/// updating passwords, and managing user rules.
/// </summary>
public interface IUserAuthContext {
    /// <summary>
    /// Gets a value indicating whether the user was found and authorized.
    /// </summary>
    public bool FoundAndAuthorized { get; }

    /// <summary>
    /// Gets IUserAuthDataContext.
    /// </summary>
    public Option<IUserAuthDataContext> UserDataContext { get; }
    /// <summary>
    /// Gets the user rule associated with the user.
    /// </summary>
    public IUserRule Rule { get; }
    /// <summary>
    /// Verifies if the given password matches the stored password.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <returns>A <see cref="ResultOk{T}"/> indicating whether the password is correct.</returns>
    public ResultOk<bool> IsPassword(string password);
    /// <summary>
    /// Checks if the given password hash matches the stored password hash for generation 1.
    /// </summary>
    /// <param name="passwordHash">The password hash to compare.</param>
    /// <returns>True if the hashes match; otherwise, false.</returns>
    public bool PasswordGen1EqualHash(string passwordHash);
    /// <summary>
    /// Validates the password and sets a generation 2 password hash if it generation 1.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="ResultOk{T}"/> indicating success or failure.</returns>
    public Task<ResultOk<bool>> IsPasswordValidAndSetGen2IfNotExistAsync(string password);
    /// <summary>
    /// Updates the user's password with new generation 1 and generation 2 hashes.
    /// </summary>
    /// <param name="password">The new password.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="ResultNone"/> indicating success or failure.</returns>
    public Task<ResultNone> UpdatePasswordAsync(string password);
}