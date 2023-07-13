namespace OsuDroidMediator.Domain.Interface; 

public interface ICookieUserInfo {
    long Id { get; }
    string Username { get; }
    string Email { get; }
    DateTime RegistTime { get; }
    string Region { get; }
    bool Active { get; }
    bool Supporter { get; }
    bool Banned { get; }
    bool RestrictMode { get; }
}