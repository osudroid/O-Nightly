namespace OsuDroidMediator.Domain.Interface; 

public interface IUpdateUsername {
    public string? NewUsername { get; }
    public string? OldUsername { get; }
    public string? Password { get; }
}