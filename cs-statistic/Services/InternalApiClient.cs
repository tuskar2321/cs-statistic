using System.Text.Json;
using tuskar.statisticApp.Models;

namespace tuskar.statisticApp.Services;

public class InternalApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("HttpClient");

    public async Task<string> Ping()
    {
        var response = await _httpClient.GetAsync(new Uri("http://localhost:8080/api/ping"));
        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return "error";
        try
        {
            var pong = JsonSerializer.Deserialize<Internal.PingResponse>(jsonResponse);
            return pong!.result;
        }
        catch (Exception)
        {
            return "error";
        }
    }
}