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
    [Inject] public SessionService Session { get; set; }
    protected string Username { get; set; }

    protected async Task ConnectAsync()
    {
        var resp = await Session.ConnectAsync(Username);

        if (!resp.IsSuccess)
            Toastr.Error(resp.Message);
        else
            Toastr.Success(resp.Message);
    }

    protected async Task DisconnectAsync()
    {
        var resp = await Session.DisconnectAsync();

        if (!resp.IsSuccess)
            Toastr.Error(resp.Message);
        else
            Toastr.Success(resp.Message);
    }
}
