using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IUserAuthDataContext {
    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Gets the password hash for the first generation of passwords.
    /// </summary>
    public string PasswordGen1 { get; }

    /// <summary>
    /// Gets the optional password hash for the second generation of passwords.
    /// </summary>
    public Option<string> PasswordGen2 { get; }
}