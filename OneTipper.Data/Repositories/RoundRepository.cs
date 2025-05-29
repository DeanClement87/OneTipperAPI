using Microsoft.EntityFrameworkCore;
using OneTipper.Data.Models;

public interface IRoundRepository
{
    Task AddAsync(Round round);
    Task DeleteAsync(Guid id);
    Task<Round> GetDetailedRoundAsync(Guid id);
    Task<IEnumerable<Round>> GetDetailedRoundsAsync();
    Task UpdateAsync(Round round);
}

public class RoundRepository : Repository<Round>, IRoundRepository
{
    private readonly OneTipperContext _context;

    public RoundRepository(OneTipperContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Round>> GetDetailedRoundsAsync()
    {
        return await _context.Rounds
            .Include(r => r.Season)
            .ToListAsync();
    }

    public async Task<Round> GetDetailedRoundAsync(Guid id)
    {
        return await _context.Rounds
            .Include(r => r.Season)
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync();
    }
}
