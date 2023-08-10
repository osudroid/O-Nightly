using System.ComponentModel.DataAnnotations;
using OsuDroidLib.Interface;

namespace OsuDroidLib.Dto;

public class PrivilegeDto : IPrototype<PrivilegeDto> {
    public PrivilegeDto() {
    }

    public PrivilegeDto(PrivilegeDto dto) {
        Id = dto.Id;
        Name = dto.Name;
        Description = dto.Description;
    }

    [Required] public Guid Id { get; init; }
    [Required] public string? Name { get; init; }
    [Required] public string? Description { get; init; }


    public PrivilegeDto CloneType() {
        return new PrivilegeDto(this);
    }
}