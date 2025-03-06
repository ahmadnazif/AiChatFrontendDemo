using AiChatFrontend.CacheServices;
using AiChatFrontend.Models;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;
using System;

namespace AiChatFrontend.Pages;

public class UserPageBase : ComponentBase
{
    [Inject] public ILogger<UserPageBase> Logger { get; set; }
    [Inject] public SessionCache SessionCache { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public ChatService2 Chat { get; set; }
    protected string Username { get; set; }

    protected async Task ConnectAsync()
    {
        if (!SessionCache.IsAuthenticated)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                Toastr.Error("Username can't be empty");
                return;
            }

            var isRegistered = await Api.IsUserRegisteredAsync(Username);

            if (isRegistered)
            {
                Toastr.Error($"User {Username} already registered");
                return;
            }

            try
            {
                await Chat.ConnectAsync(Username);
                var session = await Api.GetUserSessionByUsernameAsync(Username);
                SessionCache.Start(session);
                Toastr.Success($"Welcome {SessionCache.Session.Username}!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }
    }

    protected async Task DisconnectAsync()
    {
        if (SessionCache.IsAuthenticated)
        {
            await Chat.StopAsync();
            SessionCache.Remove();
            Username = null;
        }
    }
}
