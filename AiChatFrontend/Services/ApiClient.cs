using System.Net.Http.Headers;
using System.Text.Json;

namespace AiChatFrontend.Services;

public class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions JSON_OPT = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        this.httpClient = httpClient;
    }
}
