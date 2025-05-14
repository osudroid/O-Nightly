namespace Rimu.Web.Gen2.Feature.Login;

public struct TokenTTLDto {
    public long TTL { get; set; }

    public TokenTTLDto(long ttl) {
        TTL = ttl;
    }
}