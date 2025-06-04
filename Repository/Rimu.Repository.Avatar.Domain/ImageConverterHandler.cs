using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Avatar.Domain.Dto;
using Rimu.Repository.Avatar.Domain.ImageConverter.Interface;
using SixLabors.ImageSharp.Formats.Png;

namespace Rimu.Repository.Avatar.Domain;

/// <summary>
/// Handles image conversion operations such as resizing, format conversion, and metadata extraction.
/// </summary>
public sealed class ImageConverterHandler {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly byte[] _originalImageBytes;
    private readonly IImageTo _converter;
    public ImageConverterHandler(byte[] originalImageBytes, IImageTo converter) {
        _originalImageBytes = originalImageBytes;
        _converter = converter;
    }

    /// <summary>
    /// Retrieves the original image bytes as an <see cref="ImageDto"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ResultOk{T}"/> with the original image data.</returns>
    public ResultOk<ImageDto> CreateOriginalImageDtoAsync() {
        return CreateImageDto(_originalImageBytes);
    }
    
    public ResultOk<ImageDto> CreateLowImageDto() {
        return this.CreateImageDto(this._converter.ConvertWithLowSize(this._originalImageBytes));
    }
    
    public ResultOk<ImageDto> CreateHighImageDto() {
        return this.CreateImageDto(this._converter.ConvertWithLowSize(this._originalImageBytes));
    }
    
    /// <summary>
    /// Creates an <see cref="ImageDto"/> from the provided image bytes.
    /// </summary>
    /// <param name="bytes">The image data in byte array format.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ResultOk{T}"/> with the image metadata and data.</returns>
    private ResultOk<ImageDto> CreateImageDto(byte[] bytes) {
        try {
            var imageInfo = Image.Identify(bytes);
            var imageFormat = Image.DetectFormat(bytes);
            
            using var imageMemoryRes = new MemoryStream();
            
            return ResultOk<ImageDto>.Ok(new ImageDto() {
                Animation = imageInfo.FrameMetadataCollection.Count > 1,
                Bytes = bytes,
                PixelSize = (uint)imageInfo.Size.Width,
                TypeExt = imageFormat.DefaultMimeType,
            });
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultOk<ImageDto>.Err();
        }
    }
}