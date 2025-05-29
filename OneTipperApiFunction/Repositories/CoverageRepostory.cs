using Microsoft.EntityFrameworkCore;
using OneTipper.Data.Models;

public interface ICoverageRepostory
{
    Task<IEnumerable<Coverage>> GetAllDetailedCoverageAsync();
    Task<Coverage> GetByPlayerAndTeamAsync(Guid playerId, Guid teamId);
    Task UpdateAsync(Coverage coverage);
    Task<IEnumerable<Coverage>> GetByPlayerAsync(Guid playerId);
}

public class CoverageRepostory : Repository<Coverage>, ICoverageRepostory
{
    private readonly OneTipperContext _context;

    public CoverageRepostory(OneTipperContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Coverage>> GetAllDetailedCoverageAsync()
    {
        return await _context.Coverage
            .Include(m => m.Team)
            .Include(m => m.Player)
            .ToListAsync();
    }

    public async Task<Coverage> GetByPlayerAndTeamAsync(Guid playerId, Guid teamId)
    {
        return await _context.Coverage
            .Where(c => c.Player.Id == playerId && c.Team.Id == teamId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Coverage>> GetByPlayerAsync(Guid playerId)
    {
        return await _context.Coverage
            .Where(c => c.Player.Id == playerId)
            .ToListAsync();
    }

}
