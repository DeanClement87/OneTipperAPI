namespace OneTipper.Data.Models;

public class Tip
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public Match Match { get; set; }
    public Team Team { get; set; }
    public bool Clown { get; set; }
}
