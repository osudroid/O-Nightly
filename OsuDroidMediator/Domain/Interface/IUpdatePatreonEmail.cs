namespace OsuDroidMediator.Domain.Interface; 

public interface IUpdatePatreonEmail {
    public string? Email { get; }
    public string? Password { get; }
    public string? Username { get; }
}