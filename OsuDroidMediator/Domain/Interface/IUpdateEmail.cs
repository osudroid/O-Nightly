namespace OsuDroidMediator.Domain.Interface; 

public interface IUpdateEmail {
    public string? NewEmail { get; }
    public string? OldEmail { get; }
    public string? Password { get; }
}