using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IUserAuthDataContext {
    public string Username { get; }
    public string Email { get; }
    public long UserId { get; }
    public string PasswordGen1 { get; }
    public Option<string> PasswordGen2 { get; }
}