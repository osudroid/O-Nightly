namespace Rimu.Repository.Authentication.Adapter.Dto;

/// <summary>
/// Represents a token with its associated time-to-live (TTL) and expiration details.
/// </summary>
public struct TokenWithTTLDto {
    /// <summary>
    /// Gets the token string.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Gets the user ID associated with the token.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Gets the UTC time when the token expires.
    /// </summary>
    public DateTime DeadTimeUtc { get; }

    /// <summary>
    /// Gets the time-to-live (TTL) duration of the token.
    /// </summary>
    public TimeSpan Ttl { get; }

    /// <summary>
    /// Gets the creation time of the token by subtracting the TTL from the expiration time.
    /// </summary>
    public DateTime CreateTime => DeadTimeUtc.Subtract(Ttl);

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenWithTTLDto"/> struct.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <param name="userId">The user ID associated with the token.</param>
    /// <param name="deadTimeUtc">The UTC expiration time of the token.</param>
    /// <param name="ttl">The time-to-live (TTL) duration of the token.</param>
    public TokenWithTTLDto(string token, long userId, DateTime deadTimeUtc, TimeSpan ttl) {
        Token = token;
        UserId = userId;
        DeadTimeUtc = deadTimeUtc;
        Ttl = ttl;
    }

    /// <summary>
    /// Gets a value indicating whether the token is expired.
    /// </summary>
    public bool IsExpired => !IsEndless && DateTime.UtcNow > DeadTimeUtc;

    /// <summary>
    /// Gets a value indicating whether the token has no expiration (endless).
    /// </summary>
    public bool IsEndless => DeadTimeUtc == default;

    /// <summary>
    /// Gets the TTL of the token in seconds, rounded to the nearest integer.
    /// </summary>
    public long TTLInSeconds => Double.ConvertToInteger<long>(Double.Round(Ttl.TotalSeconds));
}