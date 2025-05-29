using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class MatchDTO
{
    public MatchDTO(Match match)
    {
        Id = match.Id;
        MatchNumber = match.MatchNumber;
        KickOff = match.KickOff;
        GameTime = match.GameTime;
        Venue = match.Venue;
        Location = match.Location;
        HomeScore = match.HomeScore;
        AwayScore = match.AwayScore;
        HomeOdds = match.HomeOdds;
        AwayOdds = match.AwayOdds;
        GameFinished = match.GameFinished;

        if (match.Round != null)
            Round = new RoundDTO(match.Round);

        if (match.HomeTeam != null)
            HomeTeam = new TeamDTO(match.HomeTeam);

        if (match.AwayTeam != null)
            AwayTeam = new TeamDTO(match.AwayTeam);

        if (match.WinningTeam != null)
        {
            if (match.WinningTeam == match.HomeTeam)
                Winners = "Home";
            else if (match.WinningTeam == match.AwayTeam)
                Winners = "Away";
        }

        if (match.Round != null && match.Round.ShowTips)
        //if (match.Round != null)
        {
            foreach (var tip in match.Tips)
            {
                var name = tip.Player.Name;
                if (tip.Clown)
                    name = $"<c>{name}";

                if (tip.Team.Id == match.HomeTeam.Id)
                {
                    HomeTippers.Add(name);
                }
                else
                {

                    AwayTippers.Add(name);
                }
            }
        }

    }

    public Guid Id { get; set; }
    public int MatchNumber { get; set; }
    public DateTime KickOff { get; set; }
    public string GameTime { get; set; }
    public string Venue { get; set; }
    public string Location { get; set; }
    public RoundDTO Round { get; set; }
    public TeamDTO HomeTeam { get; set; }
    public int HomeScore { get; set; }
    public TeamDTO AwayTeam { get; set; }
    public int AwayScore { get; set; }
    public float? HomeOdds { get; set; }
    public float? AwayOdds { get; set; }
    public bool GameFinished { get; set; }
    public string Winners { get; set; } = "";
    public List<string> HomeTippers { get; set; } = new List<string>();
    public List<string> AwayTippers { get; set; } = new List<string>();
}
