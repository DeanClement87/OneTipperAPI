using Microsoft.EntityFrameworkCore;
using OneTipper.Data.Models;

public interface ITipRepository
{
    Task AddAsync(Tip tip);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Tip>> GetAllAsync();
    Task<Tip> GetDetailedByIdAsync(Guid id);
    Task UpdateAsync(Tip tip);
    Task<IEnumerable<Tip>> GetTipsByMatchAsync(Guid matchId);
    Task<IEnumerable<Tip>> GetFinishedTipsByPlayer(Guid playerId);
    Task<IEnumerable<Tip>> GetTipsByRoundAsync(Guid roundId);
    Task<Tip> GetTipByPlayerAndRoundAsync(Guid playerId, Guid roundId);
    Task<IEnumerable<Tip>> GetTipsByPlayerAsync(Guid playerId);
}

public class TipRepository : Repository<Tip>, ITipRepository
{
    private readonly OneTipperContext _context;

    public TipRepository(OneTipperContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Tip> GetDetailedByIdAsync(Guid id)
    {
        return await _context.Tips
            .Include(m => m.Team)
            .Include(m => m.Match)
            .Include(m => m.Player)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Tip>> GetTipsByMatchAsync(Guid matchId)
    {
        return await _context.Tips
            .Include(m => m.Team)
            .Include(m => m.Match)
            .Include(m => m.Player)
            .Where(m => m.Match.Id == matchId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tip>> GetFinishedTipsByPlayer(Guid playerId)
    {
        return await _context.Tips
            .Include(m => m.Team)
            .Include(m => m.Match)
            .Include(m => m.Player)
            .Where(m => m.Player.Id == playerId && m.Match.GameFinished)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tip>> GetTipsByRoundAsync(Guid roundId)
    {
        return await _context.Tips
            .Include(m => m.Team)
            .Include(m => m.Player)
            .Where(m => m.Match.Round.Id == roundId)
            .ToListAsync();
    }

    public async Task<Tip> GetTipByPlayerAndRoundAsync(Guid playerId, Guid roundId)
    {
        return await _context.Tips
            .Include(m => m.Team)
            .Include(m => m.Match)
            .Where(m => m.Match.Round.Id == roundId && m.Player.Id == playerId)
            .OrderBy(m => m.Match.KickOff)
            .LastOrDefaultAsync();
    }

    public async Task<IEnumerable<Tip>> GetTipsByPlayerAsync(Guid playerId)
    {
        return await _context.Tips
            .Include(m => m.Team)
            .Include(m => m.Match)
                .ThenInclude(m => m.HomeTeam)
            .Include(m => m.Match)
                .ThenInclude(m => m.AwayTeam)
            .Where(m => m.Player.Id == playerId)
            .ToListAsync();
    }
}
