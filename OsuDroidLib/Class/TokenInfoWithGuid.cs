using OsuDroidLib.Database.Entities;

namespace OsuDroidLib.Class;

public readonly struct TokenInfoWithGuid {
    public readonly required Guid Token { get; init; }
    public readonly required TokenInfo TokenInfo { get; init; }

    public static implicit operator TokenInfoWithGuid(TokenUser tokenUser) {
        return new TokenInfoWithGuid {
            TokenInfo = new TokenInfo {
                CreateDay = tokenUser.CreateDate,
                UserId = tokenUser.UserId
            },
            Token = tokenUser.TokenId
        };
    }
}