using OneTipper.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneTipperApiFunction.Services;

public interface IScoreCalculator
{
    Task FullScoreUpdate(Player player);
}

public class ScoreCalculator : IScoreCalculator
{
    private readonly IRepository<Player> _playerRepository;
    private readonly ITipRepository _tipRepository;

    public ScoreCalculator(IRepository<Player> playerRepository, 
        ITipRepository tipRepository)
    {
        _playerRepository = playerRepository;
        _tipRepository = tipRepository;
    }

    public async Task FullScoreUpdate(Player player)
    {
        player.Score = 0;
        player.Points = 0;
        var playersTips = await _tipRepository.GetFinishedTipsByPlayer(player.Id);
        foreach (var playerTip in playersTips)
        {
            var diff = Math.Abs(playerTip.Match.HomeScore - playerTip.Match.AwayScore);

            if (diff == 0) //draw
            {
                player.Score += 1;
            }
            else if (playerTip.Team == playerTip.Match.WinningTeam)
            {
                player.Score += 2;
                player.Points += diff;
            }
            else if (playerTip.Team != playerTip.Match.WinningTeam)
            {
                player.Points -= diff;
            }
        }
        await _playerRepository.UpdateAsync(player);
    }

}
