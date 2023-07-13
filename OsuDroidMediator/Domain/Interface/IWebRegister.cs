namespace OsuDroidMediator.Domain.Interface; 

public interface IWebRegister {
    public string? Email { get; }
    public int MathResult { get; }
    public Guid MathToken { get; }
    public string? Region { get; }
    public string? Password { get; }
    public string? Username { get; }
}