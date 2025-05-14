using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class View_UserAvatarNoBytes: IViewUserAvatarNoBytesReadonly {
    public required long UserId { get; set; }
    public required string? Hash { get; set; }
    public required string? TypeExt { get; set; }
    public required int PixelSize { get; set; }
    public required bool Animation { get; set; }
    public required bool Original { get; set; }

    public static View_UserAvatarNoBytes FromUserAvatar(IViewUserAvatarNoBytesReadonly from) {
        return new View_UserAvatarNoBytes {
            UserId = from.UserId,
            Hash = from.Hash,
            TypeExt = from.TypeExt,
            PixelSize = from.PixelSize,
            Animation = from.Animation,
            Original = from.Original,
        };
    } 
}