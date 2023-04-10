namespace OsuDroidLib.Database.Entities; 

public class GroupPrivilegePrivilege {
    public Guid GroupPrivilegeId { get; set; }
    public bool ModeAllow { get; set; }
    public Guid PrivilegeId { get; set; }
}