using LamLibAllOver;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Avatar.Domain.Dto;

/// <summary>
/// Represents an image data transfer object (DTO) containing metadata and binary data for an image.
/// </summary>
public sealed class ImageDto {
    /// <summary>
    /// Gets the file extension or type of the image (e.g., "png", "jpg").
    /// </summary>
    public required string TypeExt { get; init; }

    /// <summary>
    /// Gets the size of the image in pixels (e.g., width or height depending on context).
    /// </summary>
    public required uint PixelSize { get; init; }

    /// <summary>
    /// Indicates whether the image contains animation (e.g., GIF).
    /// </summary>
    public required bool Animation { get; init; }

    /// <summary>
    /// Gets the binary data of the image.
    /// </summary>
    public required byte[] Bytes { get; init; }

    /// <summary>
    /// Converts the current <see cref="ImageDto"/> instance to a <see cref="UserAvatar"/> entity.
    /// </summary>
    /// <param name="userId">The ID of the user associated with the avatar.</param>
    /// <param name="isOriginal">Indicates whether the avatar is the original version.</param>
    /// <returns>A <see cref="UserAvatar"/> entity populated with the data from this DTO.</returns>
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