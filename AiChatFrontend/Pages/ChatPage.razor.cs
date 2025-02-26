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
    [Inject] public ChatService ChatClient { get; set; }
    [Inject] public CacheService Cache { get; set; }
    [Inject] public ApiClient Api { get; set; }
    protected bool IsChatting { get; set; } = false;
    protected string Username { get; set; }
    protected UserSession UserSession { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }
    protected List<ChatMessage> Chats { get; set; } = [];

    protected async Task StartChatAsync()
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

            await ChatClient.ConnectAsync(Username);
            ChatClient.OnMessageReceived += OnMessageReceived;

            UserSession = await Api.GetUserSessionByUsernameAsync(Username);
            IsChatting = true;
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex.Message);
        }
    }

    private void OnMessageReceived(object sender, EventArgs.MessageReceivedEventArgs e)
    {
        var param = e.Parameter;

        if (param == null)
            Logger.LogError("paramter is NULL");

        Chats.Add(new()
        {
            Username = param.Username,
            ConnectionId = param.ConnectionId,
            IsMine = IsMine(param),
            Message = param.ResponseMessage,
            SentTime = DateTime.Now
        });

        Logger.LogInformation($"[RECEIVED] {JsonSerializer.Serialize(param)}");
        StateHasChanged();
    }

    protected async Task SendAsync()
    {
        if(IsChatting && string.IsNullOrWhiteSpace(NewMessage))
        {
            Logger.LogInformation($"[SENT] {Cache.Username}: {NewMessage}");
            await ChatClient.SendMessageAsync(NewMessage);
            NewMessage = string.Empty;
        }
    }

    private bool IsMine(ChatHubChatResponse param)
    {
        bool isMine = false;
        if (!string.IsNullOrWhiteSpace(param.Username))
        {
            isMine = string.Equals(param.Username, Username, StringComparison.CurrentCultureIgnoreCase);
        }

        return isMine;
    }

    protected static string GetCss(bool isMine) => isMine ? "sent" : "received";

    protected async Task DisconnectAsync()
    {
        if (IsChatting)
        {
            await ChatClient.StopAsync();
            Username = null;
            UserSession = null;
            IsChatting = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }
}
