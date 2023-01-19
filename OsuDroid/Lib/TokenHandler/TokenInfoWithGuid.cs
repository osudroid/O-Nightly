using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib.TokenHandler;

public readonly struct TokenInfoWithGuid {
    public readonly required Guid Token { get; init; }
    public readonly required TokenInfo TokenInfo { get; init; }

    public static implicit operator TokenInfoWithGuid(BblTokenUser bblTokenUser) {
        return new() {
            TokenInfo = new TokenInfo {
                CreateDay = bblTokenUser.CreateDate,
                UserId = bblTokenUser.UserId
            },
            Token = bblTokenUser.TokenId
        };
    }
}