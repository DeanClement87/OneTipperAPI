namespace OneTipper.Data.Models;

public class Match
{
    public Guid Id { get; set; }
    public int MatchNumber { get; set; }
    public DateTime KickOff { get; set; }
    public string Venue { get; set; }
    public Round Round { get; set; }
    public Team HomeTeam { get; set; }
    public int HomeScore { get; set; }
    public Team AwayTeam { get; set; }
    public int AwayScore { get; set; }
    public bool GameFinished { get; set; }
    public Team? WinningTeam { get; set; }
}
