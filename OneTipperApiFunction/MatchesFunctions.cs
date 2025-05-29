using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;
using OneTipperApiFunction.Services;

namespace OneTipperApiFunction;

public class MatchesFunctions
{
    private readonly IMatchRepository _matchRepository;
    private readonly IRepository<Team> _teamRepository;
    private readonly IRepository<Round> _roundRepository;
    private readonly ITipRepository _tipRepository;
    private readonly IScoreCalculator _scoreCalculator;

    public MatchesFunctions(
        IMatchRepository matchRepository,
        IRepository<Team> teamRepository,
        IRepository<Round> roundRepository,
        ITipRepository tipRepository,
        IScoreCalculator scoreCalculator)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _roundRepository = roundRepository;
        _tipRepository = tipRepository;
        _scoreCalculator = scoreCalculator;
    }

    [Function("GetAllMatches")]
    public async Task<HttpResponseData> GetAllMatches(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "matches")] HttpRequestData req)
    {
        var matches = await _matchRepository.GetDetailedMatchesAsync();
        var matchDTOs = matches.Select(match => new MatchDTO(match)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(matchDTOs));

        return response;
    }

    [Function("GetMatchesByRound")]
    public async Task<HttpResponseData> GetMatchesByRound(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "matches/round/{roundId}")] HttpRequestData req, string roundId)
    {
        // Validate if roundId is a valid GUID
        if (!Guid.TryParse(roundId, out Guid roundGuid))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid roundId. Please provide a valid GUID.");
            return badRequestResponse;
        }

        var matches = await _matchRepository.GetDetailedMatchesByRoundAsync(roundGuid);

        var matchDTOs = matches.Select(match => new MatchDTO(match)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(matchDTOs));

        return response;
    }

    [Function("GetMatchById")]
    public async Task<HttpResponseData> GetMatchById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "matches/{id:guid}")] HttpRequestData req, Guid id)
    {
        var match = await _matchRepository.GetDetailedMatchAsync(id);
        var response = req.CreateResponse();

        if (match == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Match with ID {id} not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new MatchDTO(match)));
        return response;
    }

    [Function("CreateMatch")]
    public async Task<HttpResponseData> CreateMatch(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "matches")] HttpRequestData req)
    {
        var matchCreateModel = await JsonSerializer.DeserializeAsync<MatchCreateModel>(req.Body);

        if (matchCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid match data provided.");
            return badRequest;
        }

        var homeTeam = await _teamRepository.GetByIdAsync(matchCreateModel.HomeTeamId);
        var awayTeam = await _teamRepository.GetByIdAsync(matchCreateModel.AwayTeamId);
        var round = await _roundRepository.GetByIdAsync(matchCreateModel.RoundId);

        Team? winningTeam = null;
        if (matchCreateModel.WinningTeamId != null)
            winningTeam = await _teamRepository.GetByIdAsync(matchCreateModel.WinningTeamId.Value);

        if (homeTeam == null || awayTeam == null || round == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid team or round ID.");
            return badRequest;
        }

        var match = new Match
        {
            MatchNumber = matchCreateModel.MatchNumber,
            KickOff = matchCreateModel.KickOff,
            GameTime = matchCreateModel.GameTime,
            Venue = matchCreateModel.Venue,
            Location = matchCreateModel.Location,
            HomeTeam = homeTeam,
            HomeScore = matchCreateModel.HomeScore,
            AwayTeam = awayTeam,
            AwayScore = matchCreateModel.AwayScore,
            HomeOdds = matchCreateModel.HomeOdds,
            AwayOdds = matchCreateModel.AwayOdds,
            Round = round,
            GameFinished = matchCreateModel.GameFinished,
            WinningTeam = winningTeam,
        };

        await _matchRepository.AddAsync(match);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/matches/{match.Id}");
        await response.WriteStringAsync(JsonSerializer.Serialize(match));
        return response;
    }

    [Function("UpdateMatch")]
    public async Task<HttpResponseData> UpdateMatch(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "matches/{id:guid}")] HttpRequestData req, Guid id)
    {
        var matchCreateModel = await JsonSerializer.DeserializeAsync<MatchCreateModel>(req.Body);

        if (matchCreateModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid match data provided.");
            return badRequest;
        }

        var match = await _matchRepository.GetByIdAsync(id);

        Team? winningTeam = null;
        if (matchCreateModel.WinningTeamId != null)
            winningTeam = await _teamRepository.GetByIdAsync(matchCreateModel.WinningTeamId.Value);

        match.GameTime = matchCreateModel.GameTime;
        match.HomeScore = matchCreateModel.HomeScore;
        match.AwayScore = matchCreateModel.AwayScore;
        match.GameFinished = matchCreateModel.GameFinished;
        match.WinningTeam = winningTeam;

        if (matchCreateModel.HomeOdds != null)
            match.HomeOdds = matchCreateModel.HomeOdds;

        if (matchCreateModel.AwayOdds != null)
            match.AwayOdds = matchCreateModel.AwayOdds;

        //update winning team if game finished
        if (match.GameFinished)
        {
            if (match.HomeScore > match.AwayScore)
            {
                match.WinningTeam = match.HomeTeam;
            }
            else
            {
                match.WinningTeam = match.AwayTeam;
            }
        }

        await _matchRepository.UpdateAsync(match);

        //UPDATE SCORES
        if (match.GameFinished)
        {
            var tips = await _tipRepository.GetTipsByMatchAsync(match.Id);
            foreach (var tip in tips)
            {
                //do a full score update for every player who tipped this match
                await _scoreCalculator.FullScoreUpdate(tip.Player);
            }
        }

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteMatch")]
    public async Task<HttpResponseData> DeleteMatch(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "matches/{id:guid}")] HttpRequestData req, Guid id)
    {
        var match = await _matchRepository.GetDetailedMatchAsync(id);
        var response = req.CreateResponse();

        if (match == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Match with ID {id} not found.");
            return response;
        }

        await _matchRepository.DeleteAsync(id);

        response.StatusCode = HttpStatusCode.NoContent;
        return response;
    }


    [Function("MatchFinished")]
    public async Task<HttpResponseData> MatchFinished(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "matches/finished/{id:guid}")] HttpRequestData req, Guid id)
    {
        var matchFinishModel = await JsonSerializer.DeserializeAsync<MatchFinishModel>(req.Body);

        if (matchFinishModel == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid match data provided.");
            return badRequest;
        }

        var match = await _matchRepository.GetDetailedMatchAsync(id);

        match.HomeScore = matchFinishModel.HomeScore;
        match.AwayScore = matchFinishModel.AwayScore;
        match.GameFinished = true;

        if (match.HomeScore > match.AwayScore)
        {
            match.WinningTeam = match.HomeTeam;
        }
        else if (match.HomeScore < match.AwayScore)
        {
            match.WinningTeam = match.AwayTeam;
        }
        //draw will stay null

        await _matchRepository.UpdateAsync(match);

        //UPDATE SCORES
        var tips = await _tipRepository.GetTipsByMatchAsync(match.Id);
        foreach (var tip in tips)
        {
            //do a full score update for every player who tipped this match
            await _scoreCalculator.FullScoreUpdate(tip.Player);
        }

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }
}
