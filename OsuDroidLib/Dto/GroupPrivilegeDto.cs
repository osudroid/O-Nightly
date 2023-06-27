namespace OsuDroidLib.Dto;

public class GroupPrivilegeDto {
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Key Privileges.Id
    /// </summary>
    public Dictionary<Guid, (bool ModeAllow, PrivilegeDto Privilege)> Privileges { get; set; }
}