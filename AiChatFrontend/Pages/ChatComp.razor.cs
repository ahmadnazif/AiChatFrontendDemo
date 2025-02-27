using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class ChatCompBase : ComponentBase, IAsyncDisposable
{
    [Parameter] public ChatPageType PageType { get; set; }    
    [Inject] public ILogger<ChatCompBase> Logger { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public NavigationManager NavMan { get; set; }
    [Inject] public ChatService Chat { get; set; }
    [Inject] public ApiClient Api { get; set; }
    protected bool IsChatting { get; set; } = false;
    protected bool IsWaitingResponse { get; set; } = false;
    protected string Username { get; set; }
    protected UserSession UserSession { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }
    protected List<ChatLog> ChatLogs { get; set; } = [];

    protected async Task ReconnectAsync()
    {
        Username = UserSession.Username;
        await StartAsync();
    }

    protected async Task StartAsync()
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
            ChatLogs.Clear();

            await Chat.ConnectAsync(Username);

            Chat.OnChainedChatReceived += (s, e) =>
            {
                if (e.Response == null)
                {
                    Logger.LogError("e.Response is NULL");
                    return;
                }

                IsWaitingResponse = false;
                var resp = e.Response;

                ChatLogs.Add(new()
                {
                    Username = resp.Username,
                    ConnectionId = resp.ConnectionId,
                    Message = resp.ResponseMessage,
                    SentTime = DateTime.Now,
                    Duration = resp.Duration.ToString(),
                    ModelId = resp.ModelId
                });

                Logger.LogInformation($"[RECEIVED] {JsonSerializer.Serialize(resp)}");
                StateHasChanged();
            };

            UserSession = await Api.GetUserSessionByUsernameAsync(Username);
            IsChatting = true;
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex.Message);
        }
    }

    protected async Task SendAsync()
    {
        if (IsChatting && !string.IsNullOrWhiteSpace(NewMessage))
        {
            ChatLogs.Add(new()
            {
                ConnectionId = UserSession.ConnectionId,
                Username = UserSession.Username,
                Message = new(ChatSender.User, NewMessage),
                SentTime = DateTime.Now
            });

            var prev = ChatHelper.BuildPreviousMessages(ChatLogs);
            await Chat.SendChainedAsync(NewMessage, prev);
            Logger.LogInformation($"[SENT] {UserSession.Username}: {NewMessage}");

            NewMessage = string.Empty;
            IsWaitingResponse = true;
        }
    }

    protected async Task DisconnectAsync()
    {
        if (IsChatting)
        {
            await Chat.StopAsync();
            UserSession = new() { Username = Username };
            Username = null;
            IsChatting = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }
}

