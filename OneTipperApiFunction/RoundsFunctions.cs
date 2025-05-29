using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;

namespace OneTipperApiFunction;

public class RoundsFunctions
{
    private readonly IRoundRepository _roundRepository;
    private readonly IRepository<Season> _seasonRepository;
    private readonly ITipRepository _tipRepository;
    private readonly ICoverageRepostory _coverageRepostory;

    public RoundsFunctions(IRoundRepository roundRepository, 
        IRepository<Season> seasonRepository, 
        ITipRepository tipRepository,
        ICoverageRepostory coverageRepostory)
    {
        _roundRepository = roundRepository;
        _seasonRepository = seasonRepository;
        _tipRepository = tipRepository;
        _coverageRepostory = coverageRepostory;
    }

    [Function("GetAllRounds")]
    public async Task<HttpResponseData> GetAllRounds(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "rounds")] HttpRequestData req)
    {
        var rounds = await _roundRepository.GetDetailedRoundsAsync();

        var roundDTOs = rounds.Select(round => new RoundDTO(round)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(roundDTOs));

        return response;
    }

    [Function("GetAllRoundsBySeason")]
    public async Task<HttpResponseData> GetAllRoundsBySeason(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "rounds/season/{seasonId}")] HttpRequestData req, string seasonId)
    {
        // Validate if seasonId is a valid GUID
        if (!Guid.TryParse(seasonId, out Guid seasonGuid))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid seasonId. Please provide a valid GUID.");
            return badRequestResponse;
        }

        // Fetch rounds by seasonId (GUID)
        var rounds = await _roundRepository.GetRoundsBySeasonAsync(seasonGuid);

        var roundDTOs = rounds.Select(round => new RoundDTO(round)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(roundDTOs));

        return response;
    }

    [Function("GetRoundById")]
    public async Task<HttpResponseData> GetRoundById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "rounds/{id:guid}")] HttpRequestData req, Guid id)
    {
        var round = await _roundRepository.GetDetailedRoundAsync(id);

        var response = req.CreateResponse();

        if (round == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Round with ID {id} not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new RoundDTO(round)));

        return response;
    }

    [Function("CreateRound")]
    public async Task<HttpResponseData> CreateRound(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "rounds")] HttpRequestData req)
    {
        var roundCreateModel = await JsonSerializer.DeserializeAsync<RoundCreateModel>(req.Body);

        if (roundCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid round data provided.");
            return badRequest;
        }

        var season = await _seasonRepository.GetByIdAsync(roundCreateModel.SeasonId);

        if (season == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid season ID.");
            return badRequest;
        }

        var round = new Round
        {
            RoundNumber = int.Parse(roundCreateModel.RoundNumber),
            Season = season,
            ShowTips = roundCreateModel.ShowTips,
            RoundCutOff = roundCreateModel.RoundCutOff
        };

        await _roundRepository.AddAsync(round);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/rounds/{round.Id}");
        await response.WriteStringAsync(JsonSerializer.Serialize(round));

        return response;
    }

    [Function("UpdateRound")]
    public async Task<HttpResponseData> UpdateRound(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "rounds/{id:guid}")] HttpRequestData req, Guid id)
    {
        var roundCreateModel = await JsonSerializer.DeserializeAsync<RoundCreateModel>(req.Body);

        if (roundCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid round data provided.");
            return badRequest;
        }

        var season = await _seasonRepository.GetByIdAsync(roundCreateModel.SeasonId);

        if (season == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid season ID.");
            return badRequest;
        }

        var sydneyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
        var sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(roundCreateModel.RoundCutOff, sydneyTimeZone);
        var cutOffDay = sydneyTime.AddDays(-2);
        var cutOffDate = cutOffDay.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        var cutOffUtc = TimeZoneInfo.ConvertTimeToUtc(cutOffDate, sydneyTimeZone);

        var round = new Round
        {
            Id = id,
            RoundNumber = int.Parse(roundCreateModel.RoundNumber),
            Season = season,
            ShowTips = roundCreateModel.ShowTips,
            RoundCutOff = cutOffUtc
        };

        await _roundRepository.UpdateAsync(round);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteRound")]
    public async Task<HttpResponseData> DeleteRound(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "rounds/{id:guid}")] HttpRequestData req, Guid id)
    {
        var round = await _roundRepository.GetDetailedRoundAsync(id);
        var response = req.CreateResponse();

        if (round == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Round with ID {id} not found.");
            return response;
        }

        await _roundRepository.DeleteAsync(id);

        response.StatusCode = HttpStatusCode.NoContent;
        return response;
    }
}
