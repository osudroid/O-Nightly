namespace Rimu.Repository.Authentication.Adapter.Dto;

public struct TokenWithTTLDto {
    public string Token { get; }
    public long UserId { get; }
    public DateTime DeadTimeUtc { get; }
    public TimeSpan Ttl { get; }
    public DateTime CreateTime => DeadTimeUtc.Subtract(Ttl);

    public TokenWithTTLDto(string token, long userId, DateTime deadTimeUtc, TimeSpan ttl) {
        Token = token;
        UserId = userId;
        DeadTimeUtc = deadTimeUtc;
        Ttl = ttl;
    }

    public bool IsExpired => !IsEndless && DateTime.UtcNow > DeadTimeUtc;
    public bool IsEndless => DeadTimeUtc == default;
    
    public long TTLInSeconds => Double.ConvertToInteger<long>(Double.Round(Ttl.TotalSeconds));
}