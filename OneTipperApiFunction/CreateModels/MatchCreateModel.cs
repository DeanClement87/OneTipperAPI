using OneTipper.Data.Models;
using OneTipper.DTOs;

namespace OneTipper.CreateModels;

public class MatchCreateModel
{
    public int MatchNumber { get; set; }
    public DateTime KickOff { get; set; }
    public string GameTime { get; set; }
    public string Venue { get; set; }
    public string Location { get; set; }
    public Guid RoundId { get; set; }
    public Guid HomeTeamId { get; set; }
    public int HomeScore { get; set; }
    public Guid AwayTeamId { get; set; }
    public int AwayScore { get; set; }
    public float? HomeOdds { get; set; }
    public float? AwayOdds { get; set; }
    public bool GameFinished { get; set; }
    public Guid? WinningTeamId { get; set; }
}
