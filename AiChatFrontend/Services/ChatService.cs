
using AiChatFrontend.EventArgs;
using Microsoft.AspNetCore.SignalR.Client;

namespace AiChatFrontend.Services;

public class ChatService(IConfiguration config, ILogger<ChatService> logger, CacheService cache) : IAsyncDisposable
{
    public const string HUBURL = "/chat-hub";
    private HubConnection hubConnection;
    private readonly IConfiguration config = config;
    private readonly ILogger<ChatService> logger = logger;
    private readonly CacheService cache = cache;
    public bool IsConnected { get; set; }

    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
    public event MessageReceivedEventHandler OnMessageReceived;

    public async Task ConnectAsync(string username)
    {
        var hubUrl = $"{config["ApiUrl"].TrimEnd('/')}{HUBURL}?username={username}";
        hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).WithAutomaticReconnect().AddMessagePackProtocol().Build();
        hubConnection.Closed += (e) =>
        {
            IsConnected = false;
            LogConnectionState();
            return Task.CompletedTask;
        };
        hubConnection.Reconnected += (e) =>
        {
            IsConnected = true;
            LogConnectionState();
            return Task.CompletedTask;
        };
        hubConnection.Reconnecting += (e) =>
        {
            IsConnected = true;
            LogConnectionState();
            return Task.CompletedTask;
        };

        hubConnection.On<ChatHubChatResponse>("OnReceived", HandleReceive);

        await hubConnection.StartAsync();

        //await hubConnection.SendAsync("RegisterAsync", username);
        cache.Username = username;
        IsConnected = true;
    }

    private void HandleReceive(ChatHubChatResponse parameter) => OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(parameter));

    public async Task SendMessageAsync(string message)
    {
        ChatHubChatRequest req = new()
        {
             Message = message
        };

        try
        {
            await hubConnection.SendAsync("ReceiveMessageAsync", req);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    public async Task StopAsync()
    {
        await hubConnection.StopAsync();
        await hubConnection.DisposeAsync();
        LogConnectionState();
        hubConnection = null;
        IsConnected = false;
    }

    private void LogConnectionState()
    {
        if (hubConnection != null)
        {
            if (hubConnection.State == HubConnectionState.Connected)
                logger.LogInformation($"Hub: {hubConnection.State} [{hubConnection.ConnectionId}]");
            else
                logger.LogInformation($"Hub: {hubConnection.State}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        GC.SuppressFinalize(this);
    }
}
