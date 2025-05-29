using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;
using OneTipperApiFunction.Services;

namespace OneTipperApiFunction;

public class PlayersFunctions
{
    private readonly IPlayerRepository _repository;
    private readonly ISetupPlayerCoverage _setupPlayerCoverage;
    private readonly IScoreCalculator _scoreCalculator;
    private readonly IRepository<Season> _seasonRepository;
    private readonly IRoundRepository _roundRepository;
    private readonly ITipRepository _tipRepository;

    public PlayersFunctions(IPlayerRepository repository,
        ISetupPlayerCoverage setupPlayerCoverage,
        IScoreCalculator scoreCalculator,
        IRepository<Season> seasonRepository,
        IRoundRepository roundRepository,
        ITipRepository tipRepository)
    {
        _repository = repository;
        _setupPlayerCoverage = setupPlayerCoverage;
        _scoreCalculator = scoreCalculator;
        _seasonRepository = seasonRepository;
        _roundRepository = roundRepository;
        _tipRepository = tipRepository;
    }

    [Function("GetAllPlayers")]
    public async Task<HttpResponseData> GetAllPlayers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players")] HttpRequestData req)
    {
        var players = await _repository.GetAllAsync();

        var playerDTOs = players
            .OrderByDescending(player => player.Score)
            .ThenByDescending(player => player.Points)
            .Select(player => new PlayerDTO(player))
            .ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(playerDTOs));

        return response;
    }

    [Function("GetPlayerById")]
    public async Task<HttpResponseData> GetPlayerById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players/{id:guid}")] HttpRequestData req, Guid id)
    {
        var player = await _repository.GetByIdAsync(id);
        var response = req.CreateResponse();

        if (player == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Player with ID {id} not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new PlayerDTO(player)));

        return response;
    }

    [Function("CreatePlayer")]
    public async Task<HttpResponseData> CreatePlayer(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "players")] HttpRequestData req)
    {
        var playerCreateModel = await JsonSerializer.DeserializeAsync<PlayerCreateModel>(req.Body);

        if (playerCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid player data provided.");
            return badRequest;
        }

        var player = new Player
        {
            Name = playerCreateModel.Name,
            Seed = playerCreateModel.Seed,
            Pin = playerCreateModel.Pin,
        };

        await _repository.AddAsync(player);

        //SETUP UP PLAYER COVERAGE
        await _setupPlayerCoverage.Run(player);


        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/players/{player.Id}");
        await response.WriteStringAsync(JsonSerializer.Serialize(player));

        return response;
    }

    [Function("UpdatePlayer")]
    public async Task<HttpResponseData> UpdatePlayer(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "players/{id:guid}")] HttpRequestData req, Guid id)
    {
        var playerCreateModel = await JsonSerializer.DeserializeAsync<PlayerCreateModel>(req.Body);

        if (playerCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid player data provided.");
            return badRequest;
        }

        var player = new Player
        {
            Id = id,
            Name = playerCreateModel.Name,
            Seed = playerCreateModel.Seed,
            Pin = playerCreateModel.Pin,
        };

        await _repository.UpdateAsync(player);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeletePlayer")]
    public async Task<HttpResponseData> DeletePlayer(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "players/{id:guid}")] HttpRequestData req, Guid id)
    {
        var player = await _repository.GetByIdAsync(id);
        var response = req.CreateResponse();

        if (player == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Player with ID {id} not found.");
            return response;
        }

        await _repository.DeleteAsync(id);

        response.StatusCode = HttpStatusCode.NoContent;
        return response;
    }

    [Function("UpdateAllScores")]
    public async Task<HttpResponseData> UpdateAllScores(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "players/updatescores")] HttpRequestData req)
    {
        var players = await _repository.GetAllAsync();

        foreach (var player in players)
        {
            await _scoreCalculator.FullScoreUpdate(player);
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("GetPlayerByPin")]
    public async Task<HttpResponseData> GetPlayerByPin(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players/pin/{pin:int}")] HttpRequestData req, int pin)
    {
        var player = await _repository.GetPlayerByPinAsync(pin); 
        var response = req.CreateResponse();

        if (player == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Player with PIN {pin} not found.");
            return response;
        }

        var playerDto = new PlayerDTO(player);

        // Get live round
        var seasons = await _seasonRepository.GetAllAsync();
        var liveSeason = seasons.First(x => x.Live);

        //Get Current Tip
        var currentTip = await _tipRepository.GetTipByPlayerAndRoundAsync(player.Id, liveSeason.CurrentRoundId);
        if (currentTip != null)   
            playerDto.CurrentTip = new TipDTO(currentTip);

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(playerDto));

        return response;
    }

    [Function("CountTips")]
    public async Task<HttpResponseData> CountTips(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "players/counttips")] HttpRequestData req)
    {
        var players = await _repository.GetAllAsync();

        foreach (var playerx in players)
        {
            var player = await _repository.GetByIdAsync(playerx.Id);
            var tips = await _tipRepository.GetTipsByPlayerAsync(player.Id);

            player.HomeTips = 0;
            player.AwayTips = 0;
            foreach (var tip in tips)
            {
                if (tip.Team.Id == tip.Match.HomeTeam.Id)
                    player.HomeTips++;
                else
                    player.AwayTips++;
            }

            await _repository.UpdateAsync(player);

        }

        var response = req.CreateResponse(HttpStatusCode.Created);

        return response;
    }

}
