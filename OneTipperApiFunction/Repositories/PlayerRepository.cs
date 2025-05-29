using Microsoft.EntityFrameworkCore;
using OneTipper.Data.Models;

public interface IPlayerRepository
{
    Task<Player> GetPlayerByPinAsync(int pin);
    Task<Player> GetByIdAsync(Guid id);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Player>> GetAllAsync();
}

public class PlayerRepository : Repository<Player>, IPlayerRepository
{
    private readonly OneTipperContext _context;

    public PlayerRepository(OneTipperContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Player> GetPlayerByPinAsync(int pin)
    {
        return await _context.Players
            .Where(p => p.Pin == pin)
            .FirstOrDefaultAsync();
    }
}
