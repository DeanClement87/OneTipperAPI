using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class RoundDTO
{
    public RoundDTO(Round round)
    {
        Id = round.Id;
        RoundNumber = round.RoundNumber;
        ShowTips = round.ShowTips;
        RoundCutOff = round.RoundCutOff;

        if (round.Season != null)
            Season = new SeasonDTO(round.Season);
    }

    public Guid Id { get; set; }
    public int RoundNumber { get; set; }
    public bool ShowTips { get; set; }
    public SeasonDTO Season { get; set; }
    public DateTime RoundCutOff { get; set; }
}
