using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IUserAuthContext {
    public bool FoundAndAuthorized { get; }

    public Option<IUserAuthDataContext> UserDataContext { get; }

    public IUserRule Rule { get; }
    
    public ResultOk<bool> IsPassword(string password);
    public bool PasswordGen1EqualHash(string passwordHash);
    public Task<ResultOk<bool>> IsPasswordValidAndSetGen2IfNotExistAsync(string password);
    public Task<ResultNone> UpdatePasswordAsync(string password);
}