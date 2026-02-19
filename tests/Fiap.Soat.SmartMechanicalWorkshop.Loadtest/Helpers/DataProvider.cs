using System.Net.Http.Json;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Models;
namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Helpers;
public class DataProvider
{
    private readonly HttpClient _httpClient;
    private List<PersonDto>? _clients;
    private List<VehicleDto>? _vehicles;
    private List<AvailableServiceDto>? _availableServices;
    public DataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task LoadDataAsync()
    {
        Console.WriteLine("Loading existing data from API...");
        var peopleResponse = await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedData<PersonDto>>>("/api/v1/people?pageNumber=1&pageSize=100");
        _clients = peopleResponse?.Data.Items ?? new List<PersonDto>();
        Console.WriteLine($"Loaded {_clients.Count} clients");
        _vehicles = _clients.Where(c => c.Vehicles != null && c.Vehicles.Any()).SelectMany(c => c.Vehicles!).ToList();
        Console.WriteLine($"Loaded {_vehicles.Count} vehicles");
        var servicesResponse = await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedData<AvailableServiceDto>>>("/api/v1/availableservices?pageNumber=1&pageSize=100");
        _availableServices = servicesResponse?.Data.Items ?? new List<AvailableServiceDto>();
        Console.WriteLine($"Loaded {_availableServices.Count} available services");
        if (_clients.Count == 0 || _vehicles.Count == 0 || _availableServices.Count == 0)
        {
            throw new Exception("No data found in the database.");
        }
    }
    public PersonDto GetRandomClient()
    {
        if (_clients == null || _clients.Count == 0)
            throw new InvalidOperationException("Clients not loaded.");
        return _clients[Random.Shared.Next(_clients.Count)];
    }
    public VehicleDto GetRandomVehicle()
    {
        if (_vehicles == null || _vehicles.Count == 0)
            throw new InvalidOperationException("Vehicles not loaded.");
        return _vehicles[Random.Shared.Next(_vehicles.Count)];
    }
    public List<Guid> GetRandomServiceIds(int count = 2)
    {
        if (_availableServices == null || _availableServices.Count == 0)
            throw new InvalidOperationException("Services not loaded.");
        return _availableServices.OrderBy(_ => Random.Shared.Next()).Take(Math.Min(count, _availableServices.Count)).Select(s => s.Id).ToList();
    }
    public int GetClientCount() => _clients?.Count ?? 0;
    public int GetVehicleCount() => _vehicles?.Count ?? 0;
    public int GetServiceCount() => _availableServices?.Count ?? 0;
}
