using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Helpers;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Models;
using NBomber.Contracts;
using NBomber.CSharp;

namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Scenarios;

public class ServiceOrderLifecycleScenario
{
    private readonly DataProvider _dataProvider;
    private readonly ServiceOrderDataGenerator _dataGenerator;
    private readonly string _authToken;
    private readonly string _baseUrl;

    public ServiceOrderLifecycleScenario(DataProvider dataProvider, string authToken, string baseUrl)
    {
        _dataProvider = dataProvider;
        _dataGenerator = new ServiceOrderDataGenerator();
        _authToken = authToken;
        _baseUrl = baseUrl;
    }

    public ScenarioProps CreateScenario(int duration, int virtualUsers, int rampUpSeconds)
    {
        var scenario = Scenario.Create("service_order_lifecycle", async ctx =>
        {
            using var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            try
            {
                var clientData = _dataProvider.GetRandomClient();
                var vehicle = _dataProvider.GetRandomVehicle();
                var serviceIds = _dataProvider.GetRandomServiceIds(Random.Shared.Next(1, 3));

                var createRequest = new CreateServiceOrderRequest(
                    ClientId: clientData.Id,
                    VehicleId: vehicle.Id,
                    ServiceIds: serviceIds,
                    Title: _dataGenerator.GenerateTitle(),
                    Description: _dataGenerator.GenerateDescription()
                );

                var createResponse = await client.PostAsJsonAsync("/api/v1/serviceorders", createRequest);
                if (!createResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ServiceOrderDto>>();
                var orderId = created?.Data.Id ?? Guid.Empty;

                if (orderId == Guid.Empty)
                    return NBomber.CSharp.Response.Fail();

                await Task.Delay(Random.Shared.Next(500, 1000));

                // Step 2: UnderDiagnosis
                var underDiagnosisRequest = new PatchServiceOrderRequest("UnderDiagnosis");
                var underDiagnosisResponse = await client.PatchAsJsonAsync($"/api/v1/serviceorders/{orderId}", underDiagnosisRequest);
                if (!underDiagnosisResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                await Task.Delay(Random.Shared.Next(1000, 2000));

                // Step 3: WaitingApproval (this creates a Quote automatically)
                var waitingApprovalRequest = new PatchServiceOrderRequest("WaitingApproval");
                var waitingApprovalResponse = await client.PatchAsJsonAsync($"/api/v1/serviceorders/{orderId}", waitingApprovalRequest);
                if (!waitingApprovalResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                await Task.Delay(Random.Shared.Next(1000, 2000));

                // Step 4: Get Service Order to retrieve Quote ID
                var getOrderResponse = await client.GetAsync($"/api/v1/serviceorders/{orderId}");
                if (!getOrderResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                var orderWithQuote = await getOrderResponse.Content.ReadFromJsonAsync<ApiResponse<ServiceOrderDto>>();
                var quote = orderWithQuote?.Data.Quotes?.FirstOrDefault();

                if (quote == null || quote.Id == Guid.Empty)
                    return NBomber.CSharp.Response.Fail();

                await Task.Delay(Random.Shared.Next(500, 1000));

                // Step 5: Approve Quote (this automatically transitions to InProgress)
                var approveQuoteResponse = await client.PatchAsync($"/api/v1/serviceorders/{orderId}/quote/{quote.Id}/Approved", null);
                if (!approveQuoteResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                await Task.Delay(Random.Shared.Next(1500, 3000));

                // Step 6: Completed
                var completedRequest = new PatchServiceOrderRequest("Completed");
                var completedResponse = await client.PatchAsJsonAsync($"/api/v1/serviceorders/{orderId}", completedRequest);
                if (!completedResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                await Task.Delay(Random.Shared.Next(500, 1000));

                // Step 7: Delivered (final status)
                var deliveredRequest = new PatchServiceOrderRequest("Delivered");
                var deliveredResponse = await client.PatchAsJsonAsync($"/api/v1/serviceorders/{orderId}", deliveredRequest);
                if (!deliveredResponse.IsSuccessStatusCode)
                    return NBomber.CSharp.Response.Fail();

                return NBomber.CSharp.Response.Ok();
            }
            catch
            {
                return NBomber.CSharp.Response.Fail();
            }
        })
        .WithLoadSimulations(
            Simulation.RampingInject(rate: virtualUsers, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(rampUpSeconds)),
            Simulation.Inject(rate: virtualUsers, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(duration - rampUpSeconds))
        );

        return scenario;
    }
}
