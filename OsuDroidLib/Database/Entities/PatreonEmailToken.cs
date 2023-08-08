namespace OsuDroidLib.Database.Entities; 

public class PatreonEmailToken {
    public long UserId { get; set; }
    public Guid Token { get; set; }
    public DateTime CreateTime { get; set; }
    public string? Email { get; set; }
}