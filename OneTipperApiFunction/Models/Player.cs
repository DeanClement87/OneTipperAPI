namespace OneTipper.Data.Models;

public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public int Points { get; set; }
    public int Seed { get; set; }
    public int Pin { get; set; }
    public int HomeTips { get; set; }
    public int AwayTips { get; set; }
}
