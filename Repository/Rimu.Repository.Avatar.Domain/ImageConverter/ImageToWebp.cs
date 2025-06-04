using ImageMagick.Formats;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public class ImageToWebp: ImageTo<WebPWriteDefines> {
    public ImageToWebp(IEnvDb envDb): base(
        envDb, 
        new Lazy<WebPWriteDefines>(() => new WebPWriteDefines() {
        AlphaCompression = WebPAlphaCompression.Compressed,
        AlphaQuality = 100,
        AlphaFiltering = WebPAlphaFiltering.Best,
        FilterSharpness = 5,
        FilterType = WebPFilterType.Simple,
        Lossless = false,
        LowMemory = false,
        TargetSize = envDb.UserAvatar_ByteSizeLow,
    }, LazyThreadSafetyMode.None), new Lazy<WebPWriteDefines>(() => new WebPWriteDefines() {
            AlphaCompression = WebPAlphaCompression.Compressed,
            AlphaQuality = 100,
            AlphaFiltering = WebPAlphaFiltering.Best,
            FilterSharpness = 5,
            FilterType = WebPFilterType.Simple,
            Lossless = false,
            LowMemory = false,
            TargetSize = envDb.UserAvatar_ByteSizeHigh,
        }, LazyThreadSafetyMode.None)) {
    }

    public override byte[] ConvertWithLowSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineLow.Value, (uint)EnvDb.UserAvatar_SizeLow);
    }

    public override byte[] ConvertWithHighSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineHigh.Value, (uint)EnvDb.UserAvatar_SizeHigh);
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