using Microsoft.AspNetCore.SignalR.Client;

namespace AiChatFrontend.Services;

public class ChatService(IConfiguration config, ILogger<ChatService> logger) : IAsyncDisposable
{
    private readonly IConfiguration config = config;
    private readonly ILogger<ChatService> logger = logger;
    private HubConnection hubConnection;
    private CancellationTokenSource ctsStreaming;
    private CancellationTokenSource ctsChannel;
    private CancellationTokenSource ctsFileStreaming;
    private CancellationTokenSource ctsFileStreamingNew;
    /// <summary>
    /// Occured when single message received
    /// </summary>
    public event SingleChatReceivedEventHandler OnSingleChatReceived;

    /// <summary>
    /// Occured when single message with chained previous message received
    /// </summary>
    public event ChainedChatReceivedEventHandler OnChainedChatReceived;

    /// <summary>
    /// Occured when message received using streaming
    /// </summary>
    public event StreamingChatReceivedEventHandler OnStreamingChatReceived;

    /// <summary>
    /// Occured when messsage received using <see cref="System.Threading.Channels.ChannelReader{StreamingChatResponse}"/> streaming
    /// </summary>
    public event StreamingChatReceivedEventHandler OnChannelStreamingChatReceived;

    public event StreamingChatReceivedEventHandler OnFileStreamingChatReceived;
    public event StreamingChatReceivedEventHandler OnChatReceived;

    /// <summary>
    /// Start the connection with username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task ConnectAsync(string username)
    {
        hubConnection = HubHelper.CreateHubConnection(config, "/chat-hub", username, false);
        hubConnection.Closed += (e) =>
        {
            LogConnectionState();
            return Task.CompletedTask;
        };
        hubConnection.Reconnected += (e) =>
        {
            LogConnectionState();
            return Task.CompletedTask;
        };
        hubConnection.Reconnecting += (e) =>
        {
            LogConnectionState();
            return Task.CompletedTask;
        };

        await hubConnection.StartAsync();
    }

    public void RegisterOnReceivedSingle()
        => hubConnection.On<SingleChatResponse>("OnReceivedSingle", response => OnSingleChatReceived?.Invoke(this, new SingleChatReceivedEventArgs(response)));

    public void RegisterOnReceivedChained()
        => hubConnection.On<ChainedChatResponse>("OnReceivedChained", response => OnChainedChatReceived?.Invoke(this, new ChainedChatReceivedEventArgs(response)));


    /// <summary>
    /// Send single message (fire and forget the previous message)
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendSingleAsync(string message)
    {
        SingleChatRequest req = new()
        {
            Prompt = new(ChatSender.User, message)
        };

        try
        {
            await hubConnection.SendAsync("ReceiveSingleAsync", req);
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
            Prompt = new(ChatSender.User, message)
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

    #region Streaming chat with IAsyncEnumerable

    public async Task StartChatStreamingAsync(string message, List<ChatMsg> previousMsg)
    {
        ctsStreaming = new();

        ChainedChatRequest req = new()
        {
            PreviousMessages = previousMsg,
            Prompt = new(ChatSender.User, message)
        };

        logger.LogInformation("Streaming started");
        await foreach (var resp in hubConnection.StreamAsync<StreamingChatResponse>("StreamChatAsync", req, ctsStreaming.Token))
        {
            OnStreamingChatReceived?.Invoke(this, new StreamingChatReceivedEventArgs(resp));
        }
    }

    public void StopChatStreaming()
    {
        ctsStreaming.Cancel();
        logger.LogInformation("Streaming stopped");
    }

    #endregion

    #region Streaming chat with ChannelReader

    public async Task StartChatChannelStreamingAsync(string message, List<ChatMsg> previousMsg)
    {
        ctsChannel = new();

        ChainedChatRequest req = new()
        {
            PreviousMessages = previousMsg,
            Prompt = new(ChatSender.User, message)
        };

        logger.LogInformation("Streaming started");
        var reader = await hubConnection.StreamAsChannelAsync<StreamingChatResponse>("StreamChatAsChannel", req, ctsChannel.Token);
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var resp))
            {
                OnChannelStreamingChatReceived?.Invoke(this, new StreamingChatReceivedEventArgs(resp));
            }
        }
    }

    public void StopChatChannelStreaming()
    {
        ctsChannel.Cancel();
        logger.LogInformation("Streaming stopped");
    }

    #endregion

    #region Streaming file chat with IAsyncEnumerable

    public async Task StartFileChatStreamingAsync(string message, byte[] fileStream, string mediaType)
    {
        ctsFileStreaming = new();

        FileChatRequest req = new()
        {
            FileStream = fileStream,
            MediaType = mediaType,
            Prompt = new(ChatSender.User, message)
        };

        logger.LogInformation("Streaming started");
        await foreach (var resp in hubConnection.StreamAsync<StreamingChatResponse>("StreamFileChatAsync", req, ctsFileStreaming.Token))
        {
            OnFileStreamingChatReceived?.Invoke(this, new StreamingChatReceivedEventArgs(resp));
        }
    }

    public void StopFileChatStreaming()
    {
        ctsFileStreaming.Cancel();
        logger.LogInformation("Streaming stopped");
    }

    #endregion

    #region Streaming file chat with IAsyncEnumerable (new)

    public async Task StartStreamingAsync(string message, List<ChatFile> files, List<ChatContent> previous)
    {
        ctsFileStreamingNew = new();

        ChatRequest req = new()
        {
            Previous = previous,
            Latest = new()
            {
                Files = files,
                Message = new(ChatSender.User, message)
            },
        };

        logger.LogInformation("Streaming started");
        await foreach (var resp in hubConnection.StreamAsync<StreamingChatResponse>("StreamFileChatNewAsync", req, ctsFileStreamingNew.Token))
        {
            OnChatReceived?.Invoke(this, new StreamingChatReceivedEventArgs(resp));
        }
    }

    public void StopStreaming()
    {
        ctsFileStreamingNew.Cancel();
        logger.LogInformation("Streaming stopped");
    }

    #endregion


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
