using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiChatFrontend.Services;

public class ApiClient
{
    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        this.httpClient = httpClient;
    }

    #region /hub
    public async Task<bool> IsUserRegisteredAsync(string username)
    {
        try
        {
            var response = await httpClient.GetAsync($"rest-api/hub/user/is-username-registered?username={username}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<bool>();
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsConnectionIdActiveAsync(string connectionId)
    {
        try
        {
            var response = await httpClient.GetAsync($"rest-api/hub/user/is-connection-id-active?connectionId={connectionId}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<bool>();
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserSession> GetUserSessionByUsernameAsync(string username)
    {
        try
        {
            var response = await httpClient.GetAsync($"rest-api/hub/user/get-by-username?username={username}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserSession>();
            else
                return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<UserSession>> ListAllUserAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"rest-api/hub/user/list-all");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<UserSession>>();
            else
                return new();
        }
        catch
        {
            return new();
        }
    }

    #endregion

    #region /info
    public async Task<object> GetAiRuntimeInfoAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"rest-api/info/get-ai-runtime-info");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<object>();
            else
                return new { Status = "API not connected" };
        }
        catch (Exception ex)
        {
            return new { Exception = $"Exception: {ex.Message}" };
        }
    }
    #endregion
}
