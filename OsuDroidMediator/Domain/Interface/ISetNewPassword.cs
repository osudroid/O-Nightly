namespace OsuDroidMediator.Domain.Interface; 

public interface ISetNewPassword {
    public string? NewPassword { get; set; }
    public string? Token { get; set; }
    public long UserId { get; set; }
}