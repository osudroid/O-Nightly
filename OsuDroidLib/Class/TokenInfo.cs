using OsuDroidLib.Database.Entities;

namespace OsuDroidLib.Class; 

public struct TokenInfo {
    public required long UserId { get; init; }
    public required DateTime CreateDay { get; set; }

    public static explicit operator TokenInfo(TokenUser tokenUser) {
        return new TokenInfo {
            CreateDay = tokenUser.CreateDate,
            UserId = tokenUser.UserId
        };
    }
}