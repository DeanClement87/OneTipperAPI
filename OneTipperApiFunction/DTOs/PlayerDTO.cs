using OneTipper.Data.Models;

namespace OneTipper.DTOs;

public class PlayerDTO
{
    public PlayerDTO(Player player)
    {
        Id = player.Id;
        Name = player.Name;
        Score = player.Score;
        Points = player.Points;
        Seed = player.Seed;
        Pin = player.Pin;
        HomeTips = player.HomeTips;
        AwayTips = player.AwayTips;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public int Points { get; set; }
    public int Seed { get; set; }
    public int Pin { get; set; }
    public int HomeTips { get; set; }
    public int AwayTips { get; set; }
    public TipDTO CurrentTip { get; set; }

}
