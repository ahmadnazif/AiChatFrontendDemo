using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;
using System.Diagnostics;

namespace AiChatFrontend.Pages;

public class StreamingChatPage2Base : ComponentBase, IDisposable
{
    [Inject] public ILogger<StreamingChatPage2Base> Logger { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public NavigationManager NavMan { get; set; }
    [Inject] public ChatService2 Chat { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public CacheService Cache { get; set; }
    protected bool IsApiConnected { get; set; }
    protected bool IsChatting { get; set; } = false;
    protected bool IsStreamingCompleted { get; set; } = true;
    protected string StreamingId { get; set; }
    protected Stopwatch StreamingSw { get; set; } = new Stopwatch();
    protected string? AppendedText { get; set; }
    protected string Username { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }
    protected Dictionary<string, ChatLog> ChatLogs { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        IsApiConnected = await Api.IsConnectedAsync();
        try
        {
            Chat.OnStreamingChatReceived += OnStreamingChatReceived;
            IsChatting = true;
        }
        catch (Exception ex)
        {
            Chat.OnStreamingChatReceived -= OnStreamingChatReceived;
            Logger.LogError(ex.Message);
            IsChatting = false;
        }
    }

    private void OnStreamingChatReceived(object sender, StreamingChatReceivedEventArgs e)
    {
        var resp = e.Response;

        if (StreamingId != resp.StreamingId)
        {
            StreamingId = resp.StreamingId;
            StreamingSw.Start();

            var first = ChatHelper.BuildChatLog(ConnectionId, Username, AppendedText, resp, StreamingSw);
            ChatLogs.Add(resp.StreamingId, first);
        }

        AppendedText += resp.Message.Text;
        var current = ChatHelper.BuildChatLog(ConnectionId, Username, AppendedText, resp, StreamingSw);
        ChatHelper.AppendChatLogs(StreamingId, ChatLogs, current);

        if (resp.HasFinished)
        {
            StreamingSw.Stop();
            AppendedText = string.Empty;
            IsStreamingCompleted = true;
            Toastr.Info($"Finished at {DateTime.Now.ToLongTimeString()} [{StreamingSw.Elapsed}]");
            StreamingSw.Reset();
        }

        StateHasChanged();
    }

    protected async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
        {
            Toastr.Warning("Prompt is required.");
            return;
        }

        if (!IsChatting)
        {
            Toastr.Error("Chatting is not initiated");
            return;
        }

        var id = Generator.NextStreamingId();
        ChatLogs.Add(id, new()
        {
            ConnectionId = Cache.Session.ConnectionId,
            Username = Cache.Session.Username,
            Message = new(ChatSender.User, NewMessage),
            SentTime = DateTime.Now
        });

        var prev = ChatHelper.BuildPreviousMessages([.. ChatLogs.Values]);

        IsStreamingCompleted = false;
        Log($"[SENT] {Cache.Session.Username}: {NewMessage}");

        await Chat.StartChatStreamingAsync(NewMessage, prev);
        NewMessage = string.Empty;
    }

    protected void StopStreaming()
    {
        Chat.StopChatStreaming();
        IsStreamingCompleted = true;
    }

    protected void ClearChat()
    {
        if (!IsStreamingCompleted)
        {
            Toastr.Warning("Please wait until streaming data is completed, or click on \"Stop Streaming\" to stop first");
            return;
        }

        ChatLogs.Clear();
    }

    public void Dispose()
    {
        Chat.OnStreamingChatReceived -= OnStreamingChatReceived;
        GC.SuppressFinalize(this);
        Log("Disposed");
    }

    private void Log(string text) => Logger.LogInformation($"{DateTime.Now.ToLongTimeString()} || {text}");

    private void LogError(string text, bool isCritical = false)
    {
        var log = $"{DateTime.Now.ToLongTimeString()} || {text}";
        if (isCritical)
            Logger.LogCritical(log);
        else
            Logger.LogError(log);
    }
}

