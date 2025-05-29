namespace OneTipper.Data.Models;

public class Season
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Live { get; set; }
    public Guid CurrentRoundId { get; set; }
}
