using ImageMagick;
using ImageMagick.Formats;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public class IImageToWebp: ImageTo<WebPWriteDefines> {
    public IImageToWebp(IEnvDb envDb): base(envDb, new WebPWriteDefines() {
        AlphaCompression = WebPAlphaCompression.Compressed,
        AlphaQuality = 100,
        AlphaFiltering = WebPAlphaFiltering.Best,
        FilterSharpness = 5,
        FilterType = WebPFilterType.Simple,
        Lossless = false,
        LowMemory = false,
        TargetSize = envDb.UserAvatar_ByteSizeLow,
    }, new WebPWriteDefines() {
        AlphaCompression = WebPAlphaCompression.Compressed,
        AlphaQuality = 100,
        AlphaFiltering = WebPAlphaFiltering.Best,
        FilterSharpness = 5,
        FilterType = WebPFilterType.Simple,
        Lossless = false,
        LowMemory = false,
        TargetSize = envDb.UserAvatar_ByteSizeHigh,
    }) {
    }

    public override byte[] ConvertWithLowSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineLow, (uint)EnvDb.UserAvatar_SizeLow);
    }

    public override byte[] ConvertWithHighSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineHigh, (uint)EnvDb.UserAvatar_SizeHigh);
    }

    private byte[] Convert(byte[] fromImageBytes, WebPWriteDefines writeDefines, uint size) {
        using var images = this.LoadAndResize(fromImageBytes, size);
        
        int targetSize = writeDefines.TargetSize??0;
        var bufferSize = targetSize > 8192? targetSize / 2: 8192;
        
        using var output = new MemoryStream(bufferSize);
        
        images.Write(output, writeDefines);
        return output.ToArray();
    }
}