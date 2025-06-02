using ImageMagick;
using ImageMagick.Formats;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public class IImageToPng: ImageTo<PngWriteDefines> {

    public IImageToPng(IEnvDb envDb) : base(envDb, new PngWriteDefines() {
            BitDepth = 8,
            CompressionFilter = PngCompressionFilter.Average,
            CompressionLevel = 8,
            CompressionStrategy = PngCompressionStrategy.Default,
        }, new PngWriteDefines() {
            BitDepth = 8,
            CompressionFilter = PngCompressionFilter.Average,
            CompressionLevel = 7,
            CompressionStrategy = PngCompressionStrategy.Default,
        }
    ) {
        
    }

    public override byte[] ConvertWithLowSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineLow, (uint)EnvDb.UserAvatar_SizeLow);
    }

    public override byte[] ConvertWithHighSize(byte[] fromImageBytes) {
        return Convert(fromImageBytes, WriteDefineHigh, (uint)EnvDb.UserAvatar_SizeHigh);
    }

    private byte[] Convert(byte[] fromImageBytes, PngWriteDefines writeDefines, uint size) {
        using var images = this.LoadAndResize(fromImageBytes, size);
        
        using var output = new MemoryStream(8192);
        
        images.Write(output, writeDefines);
        return output.ToArray();
    }
}