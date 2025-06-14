using AiChatFrontend.CacheServices;
using AiChatFrontend.Pages;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Shared;

public class Component : ComponentBase
{
    [Inject] public ILogger<Component> Logger { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public ChatService Chat { get; set; }
    [Inject] public SessionCache SessionCache { get; set; }
}
