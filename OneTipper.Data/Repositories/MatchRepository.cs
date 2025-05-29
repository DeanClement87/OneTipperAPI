using Microsoft.EntityFrameworkCore;
using OneTipper.Data.Models;

public interface IMatchRepository
{
    Task<IEnumerable<Match>> GetDetailedMatchesAsync();
    Task<Match> GetDetailedMatchAsync(Guid id);
    Task AddAsync(Match match);
    Task UpdateAsync(Match match);
    Task DeleteAsync(Guid id);
}

public class MatchRepository : Repository<Match>, IMatchRepository
{
    private readonly OneTipperContext _context;

    public MatchRepository(OneTipperContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Match>> GetDetailedMatchesAsync()
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
                .ThenInclude(m => m.Season)
            .Include(m => m.WinningTeam)
            .ToListAsync();
    }

    public async Task<Match> GetDetailedMatchAsync(Guid id)
    {
        return await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Round)
                .ThenInclude(m => m.Season)
            .Include(m => m.WinningTeam)
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();
    }
}
