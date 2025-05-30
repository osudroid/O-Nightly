using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Authentication.Domain;

/// <summary>
/// Represents the user authentication data context, containing user information such as username, email, 
/// user ID, and password hashes for different generations.
/// </summary>
public class UserAuthDataContext: IUserAuthDataContext {
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

    public UserAuthDataContext(string username, string email, long userId, string passwordGen1, Option<string> passwordGen2) {
        Username = username;
        Email = email;
        UserId = userId;
        PasswordGen1 = passwordGen1;
        PasswordGen2 = passwordGen2;
    }

    /// <summary>
    /// Creates a new <see cref="UserAuthDataContext"/> instance from the given user information.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <returns>A new instance of <see cref="UserAuthDataContext"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if required user information is null.</exception>
    public static UserAuthDataContext FromUserInfo(UserInfo userInfo) {
        return new UserAuthDataContext(
            username: userInfo.Username ?? throw new NullReferenceException(), 
            email: userInfo.Email ?? throw new NullReferenceException(),
            userId: userInfo.UserId,
            passwordGen1: userInfo.Password  ?? throw new NullReferenceException(), 
            passwordGen2: userInfo.HasPasswordGen2 ? default: Option<string>.NullSplit(userInfo.PasswordGen2)
        );
    }
}