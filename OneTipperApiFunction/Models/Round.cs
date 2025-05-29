namespace OneTipper.Data.Models;

public class Round
{
    public Guid Id { get; set; }
    public int RoundNumber { get; set; }
    public bool ShowTips { get; set; }
    public Season Season { get; set; }
    public DateTime RoundCutOff { get; set; }
}
