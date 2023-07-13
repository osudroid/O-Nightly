namespace OsuDroidMediator.Domain.Interface; 

public interface IWebLogin {
    public int Math { get; }
    public Guid Token { get; }
    public string? Email { get; }
    public string? Passwd { get; }
}