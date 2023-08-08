namespace OsuDroidLib.Database.Entities; 

public class ResetPasswordKey {
    public string? Token       { get; set; }
    public long UserId      { get; set; }
    public DateTime CreateTime  { get; set; }
}