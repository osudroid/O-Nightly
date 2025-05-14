using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Rimu.Repository.Authentication.Adapter.Dto;

namespace Rimu.Repository.Authentication.Domain.Token;

public sealed class TokenGenerator {
    private readonly TimeSpan _ttl;
    public TokenGenerator(TimeSpan ttl) => _ttl = ttl;

    public TokenWithTTLDto Generate(long userId) {
        var tokenStr = RandomNumberGenerator.GetHexString(128);
        
        return new TokenWithTTLDto(tokenStr, userId, DateTime.UtcNow.Add(_ttl), _ttl);
    }

    public TokenWithTTLDto UpdateTTL(TokenWithTTLDto token) 
        => new(token.Token, token.UserId, DateTime.UtcNow.Add(_ttl), _ttl);

    public TokenWithTTLDto Create(string token, long userId, DateTime createDate) {
        return new TokenWithTTLDto(token, userId, createDate.Add(_ttl), _ttl);
    }
}