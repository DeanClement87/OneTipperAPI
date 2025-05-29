using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OneTipper.CreateModels;
using OneTipper.Data.Models;
using OneTipper.DTOs;

namespace OneTipperApiFunction;

public class TeamsFunctions
{
    private readonly IRepository<Team> _teamRepository;

    public TeamsFunctions(IRepository<Team> teamRepository)
    {
        _teamRepository = teamRepository;
    }

    [Function("GetTeams")]
    public async Task<HttpResponseData> GetTeamsData([HttpTrigger(AuthorizationLevel.Function, "get", Route = "teams")] HttpRequestData req)
    {
        var teams = await _teamRepository.GetAllAsync();

        var teamDTOs = teams
            .OrderBy(team => team.Name)
            .Select(team => new TeamDTO(team))
            .ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(teamDTOs));

        return response;
    }

    [Function("GetTeamById")]
    public async Task<HttpResponseData> GetTeamById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "teams/{id:guid}")] HttpRequestData req,
        Guid id)
    {
        var team = await _teamRepository.GetByIdAsync(id);

        var response = req.CreateResponse();

        if (team == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync($"Team with ID {id} not found.");
            return response;
        }

        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new TeamDTO(team)));

        return response;
    }

    [Function("CreateTeam")]
    public async Task<HttpResponseData> CreateTeam(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "teams")] HttpRequestData req)
    {
        var teamCreateModel = await JsonSerializer.DeserializeAsync<TeamCreateModel>(req.Body);

        if (teamCreateModel == null || string.IsNullOrEmpty(teamCreateModel.Name))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid team data provided.");
            return badRequestResponse;
        }

        var team = new Team
        {
            Name = teamCreateModel.Name,
            NrlId = teamCreateModel.NrlId,
        };

        await _teamRepository.AddAsync(team);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/teams/{team.Id}");
        await response.WriteStringAsync(JsonSerializer.Serialize(team));
        return response;
    }

    [Function("UpdateTeam")]
    public async Task<HttpResponseData> UpdateTeam(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "teams/{id:guid}")] HttpRequestData req, Guid id)
    {
        var teamCreateModel = await JsonSerializer.DeserializeAsync<TeamCreateModel>(req.Body);

        if (teamCreateModel == null || string.IsNullOrEmpty(teamCreateModel.Name))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid team data provided.");
            return badRequestResponse;
        }

        var team = await _teamRepository.GetByIdAsync(id);

        if (team == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Team with ID {id} not found.");
            return notFoundResponse;
        }

        team.Name = teamCreateModel.Name;
        team.NrlId = teamCreateModel.NrlId;
        await _teamRepository.UpdateAsync(team);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteTeam")]
    public async Task<HttpResponseData> DeleteTeam(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "teams/{id:guid}")] HttpRequestData req, Guid id)
    {
        var team = await _teamRepository.GetByIdAsync(id);

        if (team == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Team with ID {id} not found.");
            return notFoundResponse;
        }

        await _teamRepository.DeleteAsync(id);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }
}
