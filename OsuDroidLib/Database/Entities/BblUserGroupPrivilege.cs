namespace OsuDroidLib.Database.Entities; 

public class BblUserGroupPrivilege {
    public long UserId { get; set; }
    public Guid GroupPrivilegeId { get; set; }

    /*
     * key = GroupPrivilegeId
     */
    public Dictionary<Guid, GroupPrivilege>? GroupPrivilege { get; set; }
}