using OneTipper.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneTipperApiFunction.Services;

public interface ISetupPlayerCoverage
{
    Task Run(Player player);
}

public class SetupPlayerCoverage : ISetupPlayerCoverage
{
    private readonly IRepository<Team> _teamRepository;
    private readonly IRepository<Coverage> _coverageRepository;

    public SetupPlayerCoverage(IRepository<Team> teamRepository,
        IRepository<Coverage> coverageRepository)
    {
        _teamRepository = teamRepository;
        _coverageRepository = coverageRepository;
    }

    public async Task Run(Player player)
    {
        var teams = await _teamRepository.GetAllAsync();
        foreach (var team in teams)
        {
            var coverage = new Coverage()
            {
                Player = player,
                Team = team,
                TipCount = 0
            };

            await _coverageRepository.AddAsync(coverage);
        }
    }

}
