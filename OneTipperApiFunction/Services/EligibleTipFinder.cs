using OneTipper.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneTipperApiFunction.Services;

public interface IEligibleTipFinder
{
    Task<List<TeamEligiblty>> Find(Player player, Round round);
}

public class EligibleTipFinder : IEligibleTipFinder
{
    private readonly IRepository<Player> _playerRepository;
    private readonly ITipRepository _tipRepository;
    private readonly IRepository<Team> _teamRepository;
    private readonly ICoverageRepostory _coverageRepostory;
    private readonly IRoundRepository _roundRepository;
    private readonly IMatchRepository _matchRepository;

    public EligibleTipFinder(IRepository<Player> playerRepository,
        ITipRepository tipRepository,
        IRepository<Team> teamRepository,
        ICoverageRepostory coverageRepostory,
        IRoundRepository roundRepository,
        IMatchRepository matchRepository)
    {
        _playerRepository = playerRepository;
        _tipRepository = tipRepository;
        _teamRepository = teamRepository;
        _coverageRepostory = coverageRepostory;
        _roundRepository = roundRepository;
        _matchRepository = matchRepository;
    }

    public async Task<List<TeamEligiblty>> Find(Player player, Round round)
    {
        var teams = await _teamRepository.GetAllAsync();

        var teamEligiblty = new List<TeamEligiblty>();
        foreach (var team in teams)
        {
            teamEligiblty.Add(new TeamEligiblty()
            {
                Team = team
            });
        }

        //1. Check coverage
        var coverages = await _coverageRepostory.GetByPlayerAsync(player.Id);
        foreach (var coverage in coverages)
        {
            if (coverage.TipCount >= 2)
            {
                var te = teamEligiblty.First(x => x.Team == coverage.Team);
                te.Eligible = false;
                te.Reason = "maxxed";
            }
        }

        if (round.RoundNumber != 1)
        {
            //2. Check tipped last week
            var previousRound = await _roundRepository.GetByRoundNumber(round.RoundNumber - 1);

            var previousTip = await _tipRepository.GetTipByPlayerAndRoundAsync(player.Id, previousRound.Id);

            var te = teamEligiblty.First(x => x.Team == previousTip.Team);
            te.Eligible = false;
            te.Reason = "tippedlastround";

            //3. Check tipped against last week
            Team tippedAgainstLastWeek;
            if (previousTip.Match.HomeTeam == previousTip.Team)
                tippedAgainstLastWeek = previousTip.Match.AwayTeam;
            else
                tippedAgainstLastWeek = previousTip.Match.HomeTeam;

            //find current rounds match with this team, then ban the opponent
            var matches = await _matchRepository.GetDetailedMatchesByRoundAsync(round.Id);

            var match = matches.FirstOrDefault(m => m.HomeTeam == tippedAgainstLastWeek || m.AwayTeam == tippedAgainstLastWeek);
            if (match != null)
            {
                if (match.HomeTeam == tippedAgainstLastWeek)
                {
                    var tx = teamEligiblty.First(x => x.Team == match.AwayTeam);
                    tx.Eligible = false;
                    tx.Reason = "tippedagainstlastround";
                }
                else
                {
                    var tx = teamEligiblty.First(x => x.Team == match.HomeTeam);
                    tx.Eligible = false;
                    tx.Reason = "tippedagainstlastround";
                }
            }

            //4. Cant tip the bye
            foreach (var x in teamEligiblty)
            {
                bool isInMatch = matches.Any(m => m.HomeTeam.Id == x.Team.Id || m.AwayTeam.Id == x.Team.Id);

                if (!isInMatch) // If the team is NOT in any match, it's a bye
                {
                    x.Eligible = false;
                    x.Reason = "byeround";
                }
            }

            //5. Cant tip a team more than 2 times
            //TODO

        }

        return teamEligiblty;
    }

}

public class TeamEligiblty
{
    public Team Team { get; set; }
    public bool Eligible { get; set; } = true;
    public string Reason { get; set; } = string.Empty;
}
