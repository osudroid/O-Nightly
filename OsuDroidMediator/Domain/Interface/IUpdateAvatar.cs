namespace OsuDroidMediator.Domain.Interface; 

public interface IUpdateAvatar {
    public string? ImageBase64 { get; set; }
    public string? Password { get; set; }
}