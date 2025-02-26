using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class ChatPageBase : ComponentBase, IAsyncDisposable
{
    [Inject] public ILogger<ChatPageBase> Logger { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public NavigationManager NavMan { get; set; }
    [Inject] public ChatService ChatClient { get; set; }
    [Inject] public CacheService Cache { get; set; }
    protected UserSession UserSession { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }

    protected async Task StartChatAsync()
    {
        if (string.IsNullOrWhiteSpace(Cache.Username))
        {
            Toastr.Error("Username can't be empty");
            return;
        }
    }

    public ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }
}
