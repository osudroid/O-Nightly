using LamLibAllOver;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Avatar.Domain.Dto;

public sealed class ImageDto {
    public required string TypeExt { get; init; }
    public required uint PixelSize { get; init; }
    public required bool Animation { get; init; }
    public required byte[] Bytes { get; init; }

    public UserAvatar ToUserAvatar(long userId,bool isOriginal) {
        return new UserAvatar {
            Animation = this.Animation,
            Bytes = this.Bytes,
            PixelSize = (int)this.PixelSize,
            TypeExt = this.TypeExt,
            Hash = Sha3.GetSha3Byte(Bytes),
            Original = isOriginal,
            UserId = userId
        };
    }
}