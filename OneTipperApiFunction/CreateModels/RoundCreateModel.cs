using OneTipper.Data.Models;
using OneTipper.DTOs;

namespace OneTipper.CreateModels;

public class RoundCreateModel
{
    public string RoundNumber { get; set; }
    public bool ShowTips { get; set; }
    public Guid SeasonId { get; set; }
    public DateTime RoundCutOff { get; set; }
}
