using LamLibAllOver;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Authentication.Domain.Password;

/// <summary>
/// Provides functionality for hashing and verifying passwords using the first-generation password hashing algorithm.
/// </summary>
public class PasswordGen1Provider: IPasswordProvider {
    private static Option<IEnvDb> _envDb;

    /// <summary>
    /// Gets the environment database instance, initializing it if not already set.
    /// </summary>
    private static IEnvDb EnvDb {
        get {
            if (_envDb.IsNotSet()) {
                _envDb = Option<IEnvDb>.With(Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.GetEnvDb());
            }
            
            return _envDb.Unwrap();
        }
    }
    
    /// <summary>
    /// Hashes the given password using the MD5 algorithm and a password seed from the environment database.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password as a lowercase string.</returns>
    public string HashPassword(string password) => MD5.Hash(password + _envDb.Unwrap().Password_Seed).ToLower();

    /// <summary>
    /// Verifies if the given password matches the provided hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;

    /// <summary>
    /// Determines whether the given hash needs to be rehashed.
    /// </summary>
    /// <param name="hash">The hash to check.</param>
    /// <returns>Always returns false, as rehashing is not required for this provider.</returns>
    public bool NeedRehash(string hash) {
        return false;
    }
}