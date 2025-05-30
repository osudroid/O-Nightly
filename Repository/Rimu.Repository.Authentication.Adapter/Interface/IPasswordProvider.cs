namespace Rimu.Repository.Authentication.Adapter.Interface;

/// <summary>
/// Provides functionality for hashing and verifying passwords.
/// </summary>
public interface IPasswordProvider {
    /// <summary>
    /// Hashes the given password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password as a lowercase string.</returns>
    public string HashPassword(string password);
    
    /// <summary>
    /// Verifies if the given password matches the provided hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Determines whether the given hash needs to be rehashed.
    /// </summary>
    /// <param name="hash">The hash to check.</param>
    /// <returns>Determines whether the given hash needs to be rehashed based on the current salt.</returns>
    public bool NeedRehash(string hash);
}