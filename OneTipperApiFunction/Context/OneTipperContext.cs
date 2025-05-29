using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OneTipper.Data.Models;

public class OneTipperContext : DbContext
{
    private readonly IConfiguration _configuration;

    public OneTipperContext(DbContextOptions<OneTipperContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    public DbSet<Team> Teams { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Tip> Tips { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Coverage> Coverage { get; set; }
}
