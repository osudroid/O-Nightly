namespace OsuDroidLib.Database.Entities; 

public class GroupPrivilege {
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    /*
     * Key = PrivilegeId
     */
    public Dictionary<Guid, GroupPrivilegePrivilege> GroupPrivilegePrivilege { get; set; }
}