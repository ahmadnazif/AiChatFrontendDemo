using AiChatFrontend.EventArgs;
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
            UserSession = await Api.GetUserSessionByUsernameAsync(Username);

            switch (PageType)
            {
                case ChatPageType.ChainedChat: Chat.OnChainedChatReceived += this.OnChainedChatReceived; break;
                case ChatPageType.FireAndForget: Chat.OnOneChatReceived += this.OnOneChatReceived; break;
            }

            IsChatting = true;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, true);
        }
    }

    private void OnChainedChatReceived(object sender, ChainedChatReceivedEventArgs e)
    {
        if (e.Response == null)
        {
            LogError("e.Response is NULL");
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

        Log($"[RECEIVED] {JsonSerializer.Serialize(resp)}");
        StateHasChanged();
    }

    private void OnOneChatReceived(object sender, OneChatReceivedEventArgs e)
    {
        if (e.Response == null)
        {
            LogError("e.Response is NULL");
            return;
        }

        IsWaitingResponse = false;
        var resp = e.Response;

        ChatLogs.Add(new()
        {
            Username = resp.Username,
            ConnectionId = resp.ConnectionId,
            Message = new(ChatSender.Assistant, resp.ResponseMessage),
            SentTime = DateTime.Now,
            Duration = resp.Duration.ToString(),
            ModelId = resp.ModelId
        });

        Log($"[RECEIVED] {JsonSerializer.Serialize(resp)}");
        StateHasChanged();
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

            switch (PageType)
            {
                case ChatPageType.ChainedChat: await Chat.SendChainedAsync(NewMessage, prev); break;
                case ChatPageType.FireAndForget: await Chat.SendOneAsync(NewMessage); break;
            }

            Log($"[SENT] {UserSession.Username}: {NewMessage}");

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
        Log("Disposed");
    }

    private void Log(string text) => Logger.LogInformation($"{DateTime.Now.ToLongTimeString} | {PageType} || {text}");

    private void LogError(string text, bool isCritical = false)
    {
        var log = $"{DateTime.Now.ToLongTimeString} | {PageType} || {text}";
        if (isCritical)
            Logger.LogCritical(log);
        else
            Logger.LogError(log);
    }
}

