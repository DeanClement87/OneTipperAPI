using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class CoverageDTO
{
    public CoverageDTO(Coverage tip)
    {
        Id = tip.Id;
        TipCount = tip.TipCount;

        if (tip.Player != null)
            Player = new PlayerDTO(tip.Player);

        if (tip.Team != null)
            Team = new TeamDTO(tip.Team);
    }

    public Guid Id { get; set; }
    public PlayerDTO Player { get; set; }
    public TeamDTO Team { get; set; }
    public int TipCount { get; set; }
}
