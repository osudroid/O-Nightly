using ImageMagick;
using Rimu.Repository.Avatar.Domain.ImageConverter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public abstract class ImageTo<T> : IImageTo<T> where T: IWriteDefines {
    protected readonly IEnvDb EnvDb;
    protected readonly Lazy<T> WriteDefineLow;
    protected readonly Lazy<T> WriteDefineHigh;

    protected ImageTo(IEnvDb envDb, Lazy<T> lazyWriteDefineLow, Lazy<T> lazyWriteDefineHigh) {
        EnvDb = envDb;
        WriteDefineLow = lazyWriteDefineLow;
        WriteDefineHigh = lazyWriteDefineHigh;
    }

    public abstract byte[] ConvertWithLowSize(byte[] fromImageBytes);

    public abstract byte[] ConvertWithHighSize(byte[] fromImageBytes);
    
    protected MagickImageCollection LoadAndResize(byte[] fromImageBytes, uint size) {
        var images = new MagickImageCollection(fromImageBytes);

        var sizeGeo = new MagickGeometry(size, size);
        sizeGeo.IgnoreAspectRatio = true;
        
        foreach (var image in images) {
            image.Resize(sizeGeo);
        }

        return images;
    } 
}