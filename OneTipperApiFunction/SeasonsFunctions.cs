using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;

namespace OneTipperApiFunction;

public class SeasonsFunctions
{
    private readonly IRepository<Season> _repository;

    public SeasonsFunctions(IRepository<Season> repository)
    {
        _repository = repository;
    }

    [Function("GetAllSeasons")]
    public async Task<HttpResponseData> GetAllSeasons(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "seasons")] HttpRequestData req)
    {
        var seasons = await _repository.GetAllAsync();

        var seasonDTOs = seasons.Select(season => new SeasonDTO(season)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(seasonDTOs));

        return response;
    }

    [Function("GetSeasonById")]
    public async Task<HttpResponseData> GetSeasonById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "seasons/{id:guid}")] HttpRequestData req, Guid id)
    {
        var season = await _repository.GetByIdAsync(id);
        var response = req.CreateResponse();

        if (season == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Season with ID {id} not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new SeasonDTO(season)));

        return response;
    }

    [Function("GetLiveSeason")]
    public async Task<HttpResponseData> GetLiveSeason(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "seasons/live")] HttpRequestData req)
    {
        var seasons = await _repository.GetAllAsync();
        var liveSeason = seasons.FirstOrDefault(x => x.Live);
        var response = req.CreateResponse();

        if (liveSeason == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Live season not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new SeasonDTO(liveSeason)));

        return response;
    }

    [Function("CreateSeason")]
    public async Task<HttpResponseData> CreateSeason(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "seasons")] HttpRequestData req)
    {
        var seasonCreateModel = await JsonSerializer.DeserializeAsync<SeasonCreateModel>(req.Body);

        if (seasonCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid season data provided.");
            return badRequest;
        }

        var season = new Season
        {
            Name = seasonCreateModel.Name,
            Live = Convert.ToBoolean(seasonCreateModel.Live),
            CurrentRoundId = Guid.Parse(seasonCreateModel.CurrentRoundId)
        };

        await _repository.AddAsync(season);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/seasons/{season.Id}");
        await response.WriteStringAsync(JsonSerializer.Serialize(season));

        return response;
    }

    [Function("UpdateSeason")]
    public async Task<HttpResponseData> UpdateSeason(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "seasons/{id:guid}")] HttpRequestData req, Guid id)
    {
        var seasonCreateModel = await JsonSerializer.DeserializeAsync<SeasonCreateModel>(req.Body);

        if (seasonCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid season data provided.");
            return badRequest;
        }

        var season = new Season
        {
            Id = id,
            Name = seasonCreateModel.Name,
            Live = Convert.ToBoolean(seasonCreateModel.Live),
            CurrentRoundId = Guid.Parse(seasonCreateModel.CurrentRoundId)
        };

        await _repository.UpdateAsync(season);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteSeason")]
    public async Task<HttpResponseData> DeleteSeason(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "seasons/{id:guid}")] HttpRequestData req, Guid id)
    {
        var season = await _repository.GetByIdAsync(id);
        var response = req.CreateResponse();

        if (season == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Season with ID {id} not found.");
            return response;
        }

        await _repository.DeleteAsync(id);

        response.StatusCode = HttpStatusCode.NoContent;
        return response;
    }
}
