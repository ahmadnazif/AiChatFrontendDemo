using AiChatFrontend.CacheServices;
using AiChatFrontend.Pages;
using Microsoft.Extensions.Logging;
using System;

namespace AiChatFrontend.Services;

public class SessionService(SessionCache cache, ApiClient api, ChatService chat)
{
    private readonly SessionCache cache = cache;
    private readonly ApiClient api = api;
    private readonly ChatService chat = chat;

    public async Task<ResponseBase> ConnectAsync(string username)
    {
        if (cache.IsAuthenticated)
            return new()
            {
                IsSuccess = false,
                Message = "Already authenticated"
            };

        if (string.IsNullOrWhiteSpace(username))
        {
            return new()
            {
                IsSuccess = false,
                Message = "Username can't be empty"
            };
        }

        var isRegistered = await api.IsUserRegisteredAsync(username);

        if (isRegistered)
        {
            return new()
            {
                IsSuccess = false,
                Message = $"User {username} already registered"
            };
        }

        try
        {
            await chat.ConnectAsync(username);
            var session = await api.GetUserSessionByUsernameAsync(username);
            cache.Start(session);
            return new()
            {
                IsSuccess = true,
                Message = $"Welcome {cache.Session.Username}"
            };
        }
        catch (Exception ex)
        {
            return new()
            {
                IsSuccess = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<ResponseBase> DisconnectAsync()
    {
        if (!cache.IsAuthenticated)
            return new()
            {
                IsSuccess = false,
                Message = "Not authenticated yet"
            };

        await chat.StopAsync();
        cache.Remove();

        return new()
        {
            IsSuccess = true,
            Message = "Session terminated"
        };
    }
}
