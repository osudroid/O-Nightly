using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Authentication.Domain.Password;

/// <summary>
/// Provides functionality for hashing and verifying passwords using the second-generation password hashing algorithm.
/// </summary>
public class PasswordGen2Provider: IPasswordProvider {
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
    /// Hashes the given password using the BCrypt algorithm and a salt from the environment database.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password as a string.</returns>
    /// <exception cref="NullReferenceException">Thrown if the hashing process returns null.</exception>
    public string HashPassword(string password) 
        => BCrypt.Net.BCrypt.HashPassword(password, EnvDb.Password_BCryptSalt) ?? throw new NullReferenceException();

    /// <summary>
    /// Verifies if the given password matches the provided hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash) {
        return BCrypt.Net.BCrypt.Verify(hash, password);
    }

    /// <summary>
    /// Determines whether the given hash needs to be rehashed based on the current salt.
    /// </summary>
    /// <param name="hash">The hash to check.</param>
    /// <returns>True if the hash needs to be rehashed; otherwise, false.</returns>
    public bool NeedRehash(string hash) {
        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, EnvDb.Password_BCryptSalt);
    }
}