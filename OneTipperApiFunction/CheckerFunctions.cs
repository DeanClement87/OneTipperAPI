using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;
using OneTipperApiFunction.Services;

namespace OneTipperApiFunction;

public class CheckerFunctions
{
    private readonly IRepository<Season> _seasonRepository;
    private readonly IRoundRepository _roundRepository;
    private readonly ITipRepository _tipRepository;
    private readonly IRepository<Player> _playerRepository;
    private readonly ICoverageRepostory _coverageRepostory;
    private readonly IEligibleTipFinder _eligibleTipFinder;
    private readonly IMatchRepository _matchRepository;

    public CheckerFunctions(IRepository<Season> seasonRepository,
        IRoundRepository roundRepository,
        ITipRepository tipRepository,
        IRepository<Player> playerRepository,
        ICoverageRepostory coverageRepostory,
        IEligibleTipFinder eligibleTipFinder,
        IMatchRepository matchRepository)
    {
        _seasonRepository = seasonRepository;
        _roundRepository = roundRepository;
        _tipRepository = tipRepository;
        _playerRepository = playerRepository;
        _coverageRepostory = coverageRepostory;
        _eligibleTipFinder = eligibleTipFinder;
        _matchRepository = matchRepository;
    }

    [Function("Check")]
    public async Task<HttpResponseData> DoCheck([HttpTrigger(AuthorizationLevel.Function, "get", Route = "checker")] HttpRequestData req)
    {
        var seasons = await _seasonRepository.GetAllAsync();
        var liveSeason = seasons.FirstOrDefault(x => x.Live);

        var liveRound = await _roundRepository.GetByIdAsync(liveSeason.CurrentRoundId);

        //if (liveRound.RoundCutOff < DateTime.UtcNow)
        if (liveRound.RoundCutOff < new DateTime(2026, 4, 2, 12, 59, 59))
        {
            liveRound.ShowTips = true;
            await _roundRepository.UpdateAsync(liveRound);

            var players = await _playerRepository.GetAllAsync();
            var tips = await _tipRepository.GetTipsByRoundAsync(liveRound.Id);

            foreach (var player in players)
            {
                if (!tips.Any(x => x.Player == player))
                {
                    var teamEligiblty = await _eligibleTipFinder.Find(player, liveRound);

                    var matches = await _matchRepository.GetDetailedMatchesByRoundAsync(liveRound.Id);

                    Dictionary<Team, float?> dictOdds = new Dictionary<Team, float?>();
                    foreach (var match in matches)
                    {
                        dictOdds[match.HomeTeam] = match.HomeOdds;
                        dictOdds[match.AwayTeam] = match.AwayOdds;
                    }

                    var sortedTeamsByOdds = dictOdds.OrderByDescending(kvp => kvp.Value);

                    Team? eligibleTeam = null;

                    // Find the first team that is eligible
                    foreach (var kvp in sortedTeamsByOdds)
                    {
                        if (teamEligiblty.Any(te => te.Team == kvp.Key && te.Eligible))
                        {
                            eligibleTeam = kvp.Key;
                            break;
                        }
                    }

                    var eligibleMatch = matches.FirstOrDefault(m => m.HomeTeam == eligibleTeam || m.AwayTeam == eligibleTeam);

                    var clownTip = new Tip()
                    {
                        Player = player,
                        Team = eligibleTeam,
                        Match = eligibleMatch,
                        Clown = true,
                    };

                    await _tipRepository.AddAsync(clownTip);

                }
            }

            //UPDATE PLAYER COVERAGE
            var roundTips = await _tipRepository.GetTipsByRoundAsync(liveRound.Id);
            foreach (var roundTip in roundTips)
            {
                var coverage = await _coverageRepostory.GetByPlayerAndTeamAsync(roundTip.Player.Id, roundTip.Team.Id);
                coverage.TipCount += 1;
                await _coverageRepostory.UpdateAsync(coverage);
            }

            //UPDATE HOME AND AWAY COUNTERS
            foreach (var playerx in players)
            {
                var player = await _playerRepository.GetByIdAsync(playerx.Id);
                var tipsx = await _tipRepository.GetTipsByPlayerAsync(player.Id);

                player.HomeTips = 0;
                player.AwayTips = 0;
                foreach (var tip in tipsx)
                {
                    if (tip.Team.Id == tip.Match.HomeTeam.Id)
                        player.HomeTips++;
                    else
                        player.AwayTips++;
                }

                await _playerRepository.UpdateAsync(player);

            }


        }

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }


}
