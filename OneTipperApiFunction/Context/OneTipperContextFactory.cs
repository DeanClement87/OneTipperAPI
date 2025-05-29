using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

public class OneTipperContextFactory : IDesignTimeDbContextFactory<OneTipperContext>
{
    public OneTipperContext CreateDbContext(string[] args)
    {

        // Get the connection string
        var connectionString = "Server=tcp:onetippersqlserver.database.windows.net,1433;Initial Catalog=NrlOneTipper;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\"";

        // Build DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<OneTipperContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new OneTipperContext(optionsBuilder.Options);
    }
}
