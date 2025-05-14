using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Authentication.Domain;

public class UserAuthDataContext: IUserAuthDataContext {
    public string Username { get; }
    public string Email { get; }
    public long UserId { get; }
    public string PasswordGen1 { get; }
    public Option<string> PasswordGen2 { get; }

    public UserAuthDataContext(string username, string email, long userId, string passwordGen1, Option<string> passwordGen2) {
        Username = username;
        Email = email;
        UserId = userId;
        PasswordGen1 = passwordGen1;
        PasswordGen2 = passwordGen2;
    }

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