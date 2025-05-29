namespace OneTipper.Data.Models;

public class Round
{
    public Guid Id { get; set; }
    public int RoundNumber { get; set; }
    public Season Season { get; set; }
}
