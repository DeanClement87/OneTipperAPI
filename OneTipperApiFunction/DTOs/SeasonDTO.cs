using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class SeasonDTO
{
    public SeasonDTO(Season season)
    {
        Id = season.Id;
        Name = season.Name;
        Live = season.Live;
        CurrentRoundId = season.CurrentRoundId;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Live { get; set; }
    public Guid CurrentRoundId { get; set; }
}
