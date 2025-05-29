using OneTipper.Data.Models;

namespace OneTipper.CreateModels;

public class TipCreateModel
{
    public string PlayerId { get; set; }
    public string MatchId { get; set; }
    public string TeamId { get; set; }
}
