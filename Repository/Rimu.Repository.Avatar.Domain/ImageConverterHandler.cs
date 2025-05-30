using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Avatar.Domain.Dto;
using SixLabors.ImageSharp.Formats.Png;

namespace Rimu.Repository.Avatar.Domain;

/// <summary>
/// Handles image conversion operations such as resizing, format conversion, and metadata extraction.
/// </summary>
public sealed class ImageConverterHandler {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly byte[] _originalImageBytes;

    public ImageConverterHandler(byte[] originalImageBytes) {
        _originalImageBytes = originalImageBytes;
    }

    /// <summary>
    /// Retrieves the original image bytes as an <see cref="ImageDto"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ResultOk{T}"/> with the original image data.</returns>
    public async Task<ResultOk<ImageDto>> GetOriginalImageBytesAsync() {
        return await CreateImageDtoAsync(_originalImageBytes);
    }

    /// <summary>
    /// Resizes the image to the specified size and converts it to WebP format.
    /// </summary>
    /// <param name="size">The desired size (width and height) in pixels.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ResultOk{T}"/> with the resized image data.</returns>
    public async Task<ResultOk<ImageDto>> ResizeAsync(uint size) {
        using var image = Image.Load(_originalImageBytes);
        
        await using var imageMemoryRes = new MemoryStream();
        
        image.Mutate(x => { x.Resize((int) size, (int) size); });
        
        await image.SaveAsWebpAsync(imageMemoryRes, WebpEncoderSetting);

        var bytes = imageMemoryRes.ToArray();
        
        return await CreateImageDtoAsync(bytes);
    }
    
    /// <summary>
    /// Creates an <see cref="ImageDto"/> from the provided image bytes.
    /// </summary>
    /// <param name="bytes">The image data in byte array format.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ResultOk{T}"/> with the image metadata and data.</returns>
    private async Task<ResultOk<ImageDto>> CreateImageDtoAsync(byte[] bytes) {
        try {
            var imageInfo = Image.Identify(bytes);
            var imageFormat = Image.DetectFormat(bytes);
            
            await using var imageMemoryRes = new MemoryStream();
            
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

    /// <summary>
    /// Converts the image to PNG format.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ResultOk{T}"/> with the PNG image data.</returns>
    public async Task<ResultOk<ImageDto>> ToPngAsync() {
        byte[] pngBytes;
        try {
            using var image = Image.Load(_originalImageBytes);

            await using var imageMemoryRes = new MemoryStream();

            await image.SaveAsPngAsync(imageMemoryRes, PngEncoderSetting);

            pngBytes = imageMemoryRes.ToArray();
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultOk<ImageDto>.Err();
        }
        
        try {
            var imageInfo = Image.Identify(_originalImageBytes);
            var imageFormat = Image.DetectFormat(_originalImageBytes);
            
            return ResultOk<ImageDto>.Ok(new ImageDto() {
                Animation = false,
                Bytes = pngBytes,
                PixelSize = (uint)imageInfo.Size.Width,
                TypeExt = imageFormat.DefaultMimeType,
            });
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultOk<ImageDto>.Err();
        }
    }
    
    private static readonly WebpEncoder WebpEncoderSetting = new() {
        Quality = 80,
        FileFormat = WebpFileFormatType.Lossy,
        FilterStrength = 80,
        NearLossless = false,
        UseAlphaCompression = true,
        TransparentColorMode = WebpTransparentColorMode.Preserve,
        SpatialNoiseShaping = 60,
        EntropyPasses = 2,
        Method = WebpEncodingMethod.Level5
    };
    
    private static readonly PngEncoder PngEncoderSetting = new() {
        SkipMetadata = false,
        CompressionLevel = PngCompressionLevel.Level7,
        TransparentColorMode = PngTransparentColorMode.Preserve,
    };
}