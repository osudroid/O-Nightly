using LamLibAllOver;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Authentication.Domain.Password;

public class PasswordGen1Provider: IPasswordProvider {
    private static Option<IEnvDb> _envDb;

    private static IEnvDb EnvDb {
        get {
            if (_envDb.IsNotSet()) {
                _envDb = Option<IEnvDb>.With(Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.GetEnvDb());
            }
            
            return _envDb.Unwrap();
        }
    }
    
    public string HashPassword(string password) => MD5.Hash(password + _envDb.Unwrap().Password_Seed).ToLower();

    public bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;
    
    public bool NeedRehash(string hash) {
        return false;
    }
}