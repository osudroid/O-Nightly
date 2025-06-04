using ImageMagick;
using ImageMagick.Formats;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public class ImageToPng: ImageTo<PngWriteDefines> {

    public ImageToPng(IEnvDb envDb) : base(envDb, new Lazy<PngWriteDefines>(() => new PngWriteDefines() {
            BitDepth = 8,
            CompressionFilter = PngCompressionFilter.Average,
            CompressionLevel = 8,
            CompressionStrategy = PngCompressionStrategy.Default,
        }, LazyThreadSafetyMode.None), new Lazy<PngWriteDefines>(() => new PngWriteDefines() {
            BitDepth = 8,
            CompressionFilter = PngCompressionFilter.Average,
            CompressionLevel = 7,
            CompressionStrategy = PngCompressionStrategy.Default,
        }, LazyThreadSafetyMode.None)
    ) {
        
    }

    public override byte[] ConvertWithLowSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineLow.Value, (uint)EnvDb.UserAvatar_SizeLow);
    }

    public override byte[] ConvertWithHighSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineHigh.Value, (uint)EnvDb.UserAvatar_SizeHigh);
    }

    private byte[] Convert(byte[] fromImageBytes, PngWriteDefines writeDefines, uint size) {
        using var images = this.LoadAndResize(fromImageBytes, size);
        
        using var output = new MemoryStream(8192);
        
        images.Write(output, writeDefines);
        return output.ToArray();
    }
}