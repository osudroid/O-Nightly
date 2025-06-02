using ImageMagick;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public abstract class ImageTo<T>: IImageTo<T> {
    protected readonly IEnvDb EnvDb;
    protected readonly T WriteDefineLow;
    protected readonly T WriteDefineHigh;

    protected ImageTo(IEnvDb envDb, T writeDefineLow, T writeDefineHigh) {
        EnvDb = envDb;
        WriteDefineLow = writeDefineLow;
        WriteDefineHigh = writeDefineHigh;
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