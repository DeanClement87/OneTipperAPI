using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneTipperApiFunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Add Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Get the connection string from environment variables
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Environment variable 'DefaultConnection' is not set.");
        }

        // Add DbContext with the connection string
        services.AddDbContext<OneTipperContext>(options =>
            options.UseSqlServer(connectionString));

        //Add Services
        services.AddScoped<IScoreCalculator, ScoreCalculator>();
        services.AddScoped<ISetupPlayerCoverage, SetupPlayerCoverage>();
        services.AddScoped<IEligibleTipFinder, EligibleTipFinder>();

        // Add Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IRoundRepository, RoundRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<ITipRepository, TipRepository>();
        services.AddScoped<ICoverageRepostory, CoverageRepostory>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
    })
    .Build();

host.Run();
