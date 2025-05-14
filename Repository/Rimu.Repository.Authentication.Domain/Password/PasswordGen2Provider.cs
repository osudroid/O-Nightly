using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Authentication.Domain.Password;

public class PasswordGen2Provider: IPasswordProvider {
    private static Option<IEnvDb> _envDb;

    private static IEnvDb EnvDb {
        get {
            if (_envDb.IsNotSet()) {
                _envDb = Option<IEnvDb>.With(Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.GetEnvDb());
            }
            
            return _envDb.Unwrap();
        }
    }
    
    public string HashPassword(string password) 
        => BCrypt.Net.BCrypt.HashPassword(password, EnvDb.Password_BCryptSalt) ?? throw new NullReferenceException();

    public bool VerifyPassword(string password, string hash) {
        return BCrypt.Net.BCrypt.Verify(hash, password);
    }

    public bool NeedRehash(string hash) {
        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, EnvDb.Password_BCryptSalt);
    }
}