using OsuDroidLib.Database.Entities;

namespace OsuDroidLib.Dto; 

public class BblUserGroupPrivilegeDto {
    public long UserId { get; set; }
    /// <summary>
    /// Key GroupPrivilegeDto.Id
    /// </summary>
    public Dictionary<Guid, GroupPrivilegeDto> GroupPrivileges { get; set; }
}