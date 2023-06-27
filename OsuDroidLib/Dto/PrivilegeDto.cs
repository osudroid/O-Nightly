using System.ComponentModel.DataAnnotations;
using OsuDroidLib.Interface;

namespace OsuDroidLib.Dto;

public class PrivilegeDto : IPrototype<PrivilegeDto> {
    [Required] public Guid Id { get; init; }
    [Required] public string? Name { get; init; }
    [Required] public string? Description { get; init; }

    public PrivilegeDto() {
    }

    public PrivilegeDto(PrivilegeDto dto) {
        this.Id = dto.Id;
        this.Name = dto.Name;
        this.Description = dto.Description;
    }


    public PrivilegeDto CloneType() => new(this);
}