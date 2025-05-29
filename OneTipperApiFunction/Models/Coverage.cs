namespace OneTipper.Data.Models;

public class Coverage
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public Team Team { get; set; }
    public int TipCount { get; set; }
}
