namespace OsuDroidMediator.Domain.Interface; 

public interface IUpdatePassword {
    public string? NewPassword { get; }
    public string? OldPassword { get; }
}