using AiChatFrontend.Models;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;
using System;

namespace AiChatFrontend.Pages;

public class UserPageBase : ComponentBase
{
    [Inject] public ILogger<UserPageBase> Logger { get; set; }
    [Inject] public CacheService Cache { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public ChatService2 Chat { get; set; }
    protected string Username { get; set; }

    protected override async Task OnInitializedAsync()
    {
        //await StartAsync();
    }

    protected async Task StartAsync()
    {
        if (!Cache.IsAuthenticated)
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
                Cache.StartSession(session);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }
    }

    protected async Task DisconnectAsync()
    {
        if (Cache.IsAuthenticated)
        {
            await Chat.StopAsync();
            Cache.RemoveSession();
            Username = null;
        }
    }
}
