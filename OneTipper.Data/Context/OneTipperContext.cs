using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OneTipper.Data.Models;

public class OneTipperContext : DbContext
{
    private readonly IConfiguration _configuration;

    public OneTipperContext(DbContextOptions<OneTipperContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
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
}
