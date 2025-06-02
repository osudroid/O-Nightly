namespace Rimu.Repository.Avatar.Domain.ImageConverter;

public interface IImageTo {
    public byte[] ConvertWithLowSize(byte[] fromImageBytes);
    public byte[] ConvertWithHighSize(byte[] fromImageBytes);
}

public interface IImageTo<T>: IImageTo {
}