using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class UserAvatar: IViewUserAvatarNoBytesReadonly {
    public required long UserId { get; set; }
    public required string? Hash { get; set; }
    public required string? TypeExt { get; set; }
    public required int PixelSize { get; set; }
    public required bool Animation { get; set; }
    public required byte[]? Bytes { get; set; }
    public required bool Original { get; set; }
}