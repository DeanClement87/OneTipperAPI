using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class TeamDTO
{
    public TeamDTO(Team team)
    {
        Id = team.Id;
        Name = team.Name;
        NrlId = team.NrlId;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string NrlId { get; set; }
}
