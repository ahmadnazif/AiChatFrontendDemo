﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace AiChatFrontend.Services;

public class ApiClient(ILogger<ApiClient> logger, IHttpClientFactory fac)
{
    private const string NAME = nameof(ApiClient);
    private readonly ILogger<ApiClient> logger = logger;
    private readonly IHttpClientFactory fac = fac;

    #region Check Backend API
    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            var httpClient = fac.CreateClient(NAME);
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            var response = await httpClient.GetAsync($"/swagger/index.html");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }
    #endregion

    #region /hub-info
    public const string HUBINFO = "hub-info";
    public async Task<bool> IsUserRegisteredAsync(string username)
    {
        try
        {
            var httpClient = fac.CreateClient(NAME);
            var response = await httpClient.GetAsync($"{HUBINFO}/user/is-username-registered?username={username}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<bool>();
            else
                return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> IsConnectionIdActiveAsync(string connectionId)
    {
        try
        {
            var httpClient = fac.CreateClient(NAME);
            var response = await httpClient.GetAsync($"{HUBINFO}/user/is-connection-id-active?connectionId={connectionId}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<bool>();
            else
                return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<UserSession> GetUserSessionByUsernameAsync(string username)
    {
        try
        {
            var httpClient = fac.CreateClient(NAME);
            var response = await httpClient.GetAsync($"{HUBINFO}/user/get-by-username?username={username}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserSession>();
            else
                return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return null;
        }
    }

    public async Task<List<UserSession>> ListAllUserAsync()
    {
        try
        {
            var httpClient = fac.CreateClient(NAME);
            var response = await httpClient.GetAsync($"{HUBINFO}/user/list-all");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<UserSession>>();
            else
                return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return [];
        }
    }

    #endregion

    #region /app-info
    public const string APPINFO = "app-info";
    public async Task<object> GetAiRuntimeInfoAsync()
    {
        try
        {
            var httpClient = fac.CreateClient(NAME);
            var response = await httpClient.GetAsync($"{APPINFO}/get-ai-runtime-info");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<object>();
            else
                return new { Status = "API not connected" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new { Exception = $"{ex.Message}" };
        }
    }
    #endregion
}
