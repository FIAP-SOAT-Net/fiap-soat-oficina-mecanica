using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Models;
namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Helpers;
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;
    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
    public async Task<string> AuthenticateAsync(string email, string password)
    {
        var loginRequest = new LoginRequest(email, password);
        var response = await _httpClient.PostAsJsonAsync("/auth/login", loginRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Authentication failed: {response.StatusCode}");
        }
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _authToken = loginResponse?.Data ?? throw new Exception("Token not found in response");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        return _authToken;
    }
    public string? GetToken() => _authToken;
    public HttpClient GetHttpClient() => _httpClient;
}
