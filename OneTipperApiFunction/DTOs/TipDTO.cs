using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class TipDTO
{
    public TipDTO(Tip tip)
    {
        Id = tip.Id;

        if (tip.Player != null)
            Player = new PlayerDTO(tip.Player);

        if (tip.Match != null)
        {
            tip.Match.Tips = new List<Tip>();
            Match = new MatchDTO(tip.Match);
        }

        if (tip.Team != null)
            Team = new TeamDTO(tip.Team);
    }

    public Guid Id { get; set; }
    public PlayerDTO Player { get; set; }
    public MatchDTO Match { get; set; }
    public TeamDTO Team { get; set; }
}
