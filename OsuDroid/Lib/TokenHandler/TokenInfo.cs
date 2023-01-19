using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib.TokenHandler;

public struct TokenInfo {
    public required long UserId { get; init; }
    public required DateTime CreateDay { get; set; }

    public static explicit operator TokenInfo(BblTokenUser bblTokenUser) {
        return new TokenInfo {
            CreateDay = bblTokenUser.CreateDate,
            UserId = bblTokenUser.UserId
        };
    }
}