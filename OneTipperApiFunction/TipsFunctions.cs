using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;
using OneTipperApiFunction.Services;

namespace OneTipperApiFunction;

public class TipsFunctions
{
    private readonly ITipRepository _repository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRepository<Team> _teamRepository;
    private readonly IRepository<Season> _seasonRepository;
    private readonly IEligibleTipFinder _eligibleTipFinder;
    private readonly IRoundRepository _roundRepository;

    public TipsFunctions(ITipRepository repository, 
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository, 
        IRepository<Team> teamRepository,
        IRepository<Season> seasonRepository,
        IEligibleTipFinder eligibleTipFinder,
        IRoundRepository roundRepository)
    {
        _repository = repository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _eligibleTipFinder = eligibleTipFinder;
        _roundRepository = roundRepository;
    }

    [Function("GetAllTips")]
    public async Task<HttpResponseData> GetAllTips(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tips")] HttpRequestData req)
    {
        var tips = await _repository.GetAllAsync();
        var tipDTOs = tips.Select(tip => new TipDTO(tip)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(tipDTOs));

        return response;
    }

    [Function("GetTipById")]
    public async Task<HttpResponseData> GetTipById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tips/{id:guid}")] HttpRequestData req, Guid id)
    {
        var tip = await _repository.GetDetailedByIdAsync(id);
        var response = req.CreateResponse();

        if (tip == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Tip with ID {id} not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new TipDTO(tip)));

        return response;
    }

    [Function("CreateTip")]
    public async Task<HttpResponseData> CreateTip(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "tips")] HttpRequestData req)
    {
        var tipCreateModel = await JsonSerializer.DeserializeAsync<TipCreateModel>(req.Body);

        if (tipCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid tip data provided.");
            return badRequest;
        }

        //GET MATCH
        var matchId = Guid.Parse(tipCreateModel.MatchId);
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match with ID {matchId} not found.");
            return notFoundResponse;
        }

        //GET TEAM
        var teamId = Guid.Parse(tipCreateModel.TeamId); 
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Team with ID {teamId} not found.");
            return notFoundResponse;
        }

        //GET PLAYER
        var playerId = Guid.Parse(tipCreateModel.PlayerId); // Convert string to Guid
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Player with ID {playerId} not found.");
            return notFoundResponse;
        }

        var tip = new Tip
        {
            Match = match,
            Team = team,
            Player = player,
        };

        await _repository.AddAsync(tip);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/tips/{tip.Id}");
        await response.WriteStringAsync(JsonSerializer.Serialize(new TipDTO(tip)));

        return response;
    }

    [Function("UpdateTip")]
    public async Task<HttpResponseData> UpdateTip(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "tips/{id:guid}")] HttpRequestData req, Guid id)
    {
        var tipCreateModel = await JsonSerializer.DeserializeAsync<TipCreateModel>(req.Body);

        if (tipCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid tip data provided.");
            return badRequest;
        }

        //GET MATCH
        var matchId = Guid.Parse(tipCreateModel.MatchId);
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match with ID {matchId} not found.");
            return notFoundResponse;
        }

        //GET TEAM
        var teamId = Guid.Parse(tipCreateModel.TeamId);
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Team with ID {teamId} not found.");
            return notFoundResponse;
        }

        //GET PLAYER
        var playerId = Guid.Parse(tipCreateModel.PlayerId); // Convert string to Guid
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Player with ID {playerId} not found.");
            return notFoundResponse;
        }

        var tip = await _repository.GetDetailedByIdAsync(id);

        tip.Match = match;
        tip.Team = team;
        tip.Player = player;

        await _repository.UpdateAsync(tip);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteTip")]
    public async Task<HttpResponseData> DeleteTip(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "tips/{id:guid}")] HttpRequestData req, Guid id)
    {
        var tip = await _repository.GetDetailedByIdAsync(id);
        var response = req.CreateResponse();

        if (tip == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Tip with ID {id} not found.");
            return response;
        }

        await _repository.DeleteAsync(id);

        response.StatusCode = HttpStatusCode.NoContent;
        return response;
    }

    [Function("GetEligibleTips")]
    public async Task<HttpResponseData> GetEligibleTips(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tips/eligible/{pin:int}")] HttpRequestData req, int pin)
    {
        var player = await _playerRepository.GetPlayerByPinAsync(pin);

        if (player == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            return notFoundResponse;
        }

        // Get live round
        var seasons = await _seasonRepository.GetAllAsync();
        var liveSeason = seasons.First(x => x.Live);
        var round = await _roundRepository.GetByIdAsync(liveSeason.CurrentRoundId);

        if (round.ShowTips == true)
        {
            var responsex = req.CreateResponse(HttpStatusCode.OK);
            return responsex;
        }

        // Find team eligibility
        var teamEligibility = await _eligibleTipFinder.Find(player, round);

        // Create response
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(JsonSerializer.Serialize(teamEligibility));

        return response;
    }

    [Function("SubmitTip")]
    public async Task<HttpResponseData> SubmitTip(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "tips/submitTip")] HttpRequestData req)
    {
        var tipCreateModel = await JsonSerializer.DeserializeAsync<TipCreateModel>(req.Body);

        // Get live round
        var seasons = await _seasonRepository.GetAllAsync();
        var liveSeason = seasons.First(x => x.Live);
        var round = await _roundRepository.GetByIdAsync(liveSeason.CurrentRoundId);

        //Get Match
        var teamIdGuid = Guid.Parse(tipCreateModel.TeamId);
        var matches = await _matchRepository.GetDetailedMatchesByRoundAsync(round.Id);
        var selectedMatch = matches.FirstOrDefault(m => m.HomeTeam.Id == teamIdGuid || m.AwayTeam.Id == teamIdGuid);

        //Get Player
        var playerGuid = Guid.Parse(tipCreateModel.PlayerId);
        var player = await _playerRepository.GetByIdAsync(playerGuid);

        //Get Team
        var team = await _teamRepository.GetByIdAsync(teamIdGuid);

        //Get Tip if it exists already
        var tip = await _repository.GetTipByPlayerAndRoundAsync(player.Id, round.Id);
        if (tip == null )
        {
            tip = new Tip
            {
                Match = selectedMatch,
                Team = team,
                Player = player,
            };
            await _repository.AddAsync(tip);
        }
        else
        {
            tip.Team = team;
            tip.Match = selectedMatch;

            await _repository.UpdateAsync(tip);
        }

        // Create response
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(JsonSerializer.Serialize(new TipDTO(tip)));

        return response;
    }
}
