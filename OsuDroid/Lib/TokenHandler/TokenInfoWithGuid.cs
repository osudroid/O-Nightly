using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib.TokenHandler;

public readonly struct TokenInfoWithGuid {
    public readonly required Guid Token { get; init; }
    public readonly required TokenInfo TokenInfo { get; init; }

    public static implicit operator TokenInfoWithGuid(TokenUser tokenUser) {
        return new() {
            TokenInfo = new TokenInfo {
                CreateDay = tokenUser.CreateDate,
                UserId = tokenUser.UserId
            },
            Token = tokenUser.TokenId
        };
    }
}