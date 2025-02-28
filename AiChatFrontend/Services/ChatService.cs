using AiChatFrontend.EventArgs;
using AiChatFrontend.EventHandlers;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection.Metadata;

namespace AiChatFrontend.Services;

public class ChatService(IConfiguration config, ILogger<ChatService> logger) : IAsyncDisposable
{
    public const string HUBURL = "/chat-hub";
    private HubConnection hubConnection;
    private readonly IConfiguration config = config;
    private readonly ILogger<ChatService> logger = logger;

    public bool IsConnected { get; set; }

    /// <summary>
    /// Occured when one message received
    /// </summary>
    public event SingleChatReceivedEventHandler OnOneChatReceived;

    /// <summary>
    /// Occured when one message with chained previous message received
    /// </summary>
    public event ChainedChatReceivedEventHandler OnChainedChatReceived;

    /// <summary>
    /// Start the connection with username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
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

        hubConnection.On<SingleChatResponse>("OnReceivedOne", response => OnOneChatReceived?.Invoke(this, new OneChatReceivedEventArgs(response)));
        hubConnection.On<ChainedChatResponse>("OnReceivedChained", response => OnChainedChatReceived?.Invoke(this, new ChainedChatReceivedEventArgs(response)));

        await hubConnection.StartAsync();
        IsConnected = true;
    }

    /// <summary>
    /// Send one message (fire and forget the previous message)
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendOneAsync(string message)
    {
        SingleChatRequest req = new()
        {
            Message = new(ChatSender.User, message)
        };

        try
        {
            await hubConnection.SendAsync("ReceiveOneAsync", req);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Send one message with previous messages and response message attached
    /// </summary>
    /// <param name="message"></param>
    /// <param name="previousMsg"></param>
    /// <returns></returns>
    public async Task SendChainedAsync(string message, List<ChatMsg> previousMsg)
    {
        ChainedChatRequest req = new()
        {
            PreviousMessages = previousMsg,
            LatestMessage = new(ChatSender.User, message)
        };

        try
        {
            await hubConnection.SendAsync("ReceiveChainedAsync", req);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Stop the connection and dispose the hub
    /// </summary>
    /// <returns></returns>
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
