namespace OsuDroidMediator.Domain.Interface; 

public interface IWebLoginWithUsername {
    public int Math { get; }
    public Guid Token { get; }
    public string? Username { get; }
    public string? Password { get; }
}