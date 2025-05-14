namespace Rimu.Terminal.ImportCSVFile;

internal struct bbl_user {
    public required long id { get; set; }
    public required string? username { get; set; }
    public required string? password { get; set; }
    public required string? email { get; set; }
    public required string? deviceid { get; set; }
    
    public required long score { get; set; }
    public required double pp { get; set; }
    public required long playcount { get; set; }
    public required double accuracy { get; set; }
    
    public required DateTime regist_time { get; set; }
    public required DateTime last_login_time { get; set; }
    public required string? regist_ip { get; set; }
    public required string? region { get; set; }
    public required long active { get; set; }
    public required long supporter { get; set; }
    public required long core_developer { get; set; }
    public required long developer { get; set; }
    public required long contributor { get; set; }
    public required long banned { get; set; }
    public required long restrict_mode { get; set; }
    public required long archived { get; set; }
    public required long privacy_mode { get; set; }
}