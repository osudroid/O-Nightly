namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IViewUserAvatarNoBytesReadonly {
    public long UserId { get; }
    public string? Hash { get; }
    public string? TypeExt { get; }
    public int PixelSize { get; }
    public bool Animation { get; }
    public bool Original { get; }
}