using System.Net.Http.Headers;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Helpers;
using NBomber.Contracts;
using NBomber.CSharp;

namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Scenarios;

public class ReadOperationsScenario
{
    private readonly DataProvider _dataProvider;
    private readonly string _authToken;
    private readonly string _baseUrl;

    public ReadOperationsScenario(DataProvider dataProvider, string authToken, string baseUrl)
    {
        _dataProvider = dataProvider;
        _authToken = authToken;
        _baseUrl = baseUrl;
    }

    public ScenarioProps CreateScenario(int duration, int virtualUsers, int rampUpSeconds)
    {
        var scenario = Scenario.Create("read_operations", async ctx =>
        {
            using var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            try
            {
                var endpoints = new[]
                {
                    "/api/v1/people?pageNumber=1&pageSize=10",
                    "/api/v1/vehicles?pageNumber=1&pageSize=10",
                    "/api/v1/availableservices?pageNumber=1&pageSize=10",
                    "/api/v1/serviceorders?pageNumber=1&pageSize=10"
                };

                var endpoint = endpoints[Random.Shared.Next(endpoints.Length)];
                var response = await client.GetAsync(endpoint);

                return response.IsSuccessStatusCode ? NBomber.CSharp.Response.Ok() : NBomber.CSharp.Response.Fail();
            }
            catch
            {
                return NBomber.CSharp.Response.Fail();
            }
        })
        .WithLoadSimulations(
            Simulation.RampingInject(rate: virtualUsers / 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(rampUpSeconds)),
            Simulation.Inject(rate: virtualUsers / 2, interval: TimeSpan.FromSeconds(2), during: TimeSpan.FromSeconds(duration - rampUpSeconds))
        );

        return scenario;
    }
}
