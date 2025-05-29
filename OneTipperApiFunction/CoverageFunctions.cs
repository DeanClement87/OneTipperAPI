using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;

namespace OneTipperApiFunction;

public class CoverageFunctions
{
    private readonly ICoverageRepostory _repository;

    public CoverageFunctions(ICoverageRepostory repository)
    {
        _repository = repository;
    }

    [Function("GetAllCoverage")]
    public async Task<HttpResponseData> GetAllCoverage(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "coverage")] HttpRequestData req)
    {
        var coverages = await _repository.GetAllDetailedCoverageAsync();

        // Group coverages by Player
        var groupedCoverages = coverages
            .GroupBy(c => c.Player)
            .Select(group => new
            {
                Player = new PlayerDTO(group.Key), // Convert Player to DTO
                Coverages = group
                    .OrderBy(c => c.Team.Name) // Sort coverages by Team Name (Alphabetically)
                    .Select(c => new CoverageDTO(c))
                    .ToList()
            })
            .ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(groupedCoverages));

        return response;
    }

}
