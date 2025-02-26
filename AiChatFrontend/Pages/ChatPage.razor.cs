using AiChatFrontend.EventArgs;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;
using System.Text.Json.Schema;

namespace AiChatFrontend.Pages;

public class ChatPageBase : ComponentBase, IAsyncDisposable
{
    [Inject] public ILogger<ChatPageBase> Logger { get; set; }
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
    protected List<ChatMessage> Chats { get; set; } = [];

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
            Chats.Clear();

            await Chat.ConnectAsync(Username);
            Chat.OnMessageReceived += OnMessageReceived;

            UserSession = await Api.GetUserSessionByUsernameAsync(Username);
            IsChatting = true;
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex.Message);
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if (e.Parameter == null)
        {
            Logger.LogError("paramter is NULL");
            return;
        }

        IsWaitingResponse = false;
        var param = e.Parameter;       

        Chats.Add(new()
        {
            Sender = ChatSender.Assistant,
            Username = param.Username,
            ConnectionId = param.ConnectionId,
            Message = param.ResponseMessage,
            SentTime = DateTime.Now,
            Duration = param.Duration.ToString(),
            ModelId = param.ModelId
        });

        Logger.LogInformation($"[RECEIVED] {JsonSerializer.Serialize(param)}");
        StateHasChanged();
    }

    protected async Task SendAsync()
    {
        if (IsChatting && !string.IsNullOrWhiteSpace(NewMessage))
        {
            Chats.Add(new()
            {
                Sender = ChatSender.User,
                ConnectionId = UserSession.ConnectionId,
                Username = UserSession.Username,
                Message = NewMessage,
                SentTime = DateTime.Now
            });

            await Chat.SendOneAsync(NewMessage);
            Logger.LogInformation($"[SENT] {UserSession.Username}: {NewMessage}");

            NewMessage = string.Empty;
            IsWaitingResponse = true;
        }
    }

    protected static string GetUsername(ChatMessage msg)
    {
        return msg.Sender switch
        {
            ChatSender.User => msg.Username,
            ChatSender.Assistant => msg.Sender.ToString(),
            _ => string.Empty,
        };
    }

    protected static string GetChatFooter(ChatMessage msg)
    {
        return msg.Sender switch
        {
            ChatSender.Assistant => $"{msg.SentTime.ToLongTimeString()}  |  Time taken: {msg.Duration}  |  Model: {msg.ModelId}",
            ChatSender.User => msg.SentTime.ToLongTimeString(),
            _ => string.Empty,
        };
    }

    protected static string GetCss(ChatSender sender) => sender switch
    {
        ChatSender.Assistant => "received",
        ChatSender.User => "sent",
        _ => string.Empty,
    };

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
