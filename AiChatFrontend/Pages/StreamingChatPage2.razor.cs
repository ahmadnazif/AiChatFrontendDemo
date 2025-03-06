using AiChatFrontend.CacheServices;
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
    [Inject] public SessionCache SessionCache { get; set; }
    [Inject] public ChatCache ChatCache { get; set; }
    protected bool IsApiConnected { get; set; }
    protected bool IsStreamingCompleted { get; set; } = true;
    protected string StreamingId { get; set; }
    protected Stopwatch StreamingSw { get; set; } = new Stopwatch();
    protected string? AppendedText { get; set; }
    //protected string Username { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }
    //protected Dictionary<string, ChatLog> ChatLogs { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        IsApiConnected = await Api.IsConnectedAsync();
        try
        {
            Chat.OnStreamingChatReceived += OnStreamingChatReceived;
        }
        catch (Exception ex)
        {
            Chat.OnStreamingChatReceived -= OnStreamingChatReceived;
            LogError(ex.Message);
        }
    }

    private void OnStreamingChatReceived(object sender, StreamingChatReceivedEventArgs e)
    {
        var resp = e.Response;

        if (StreamingId != resp.StreamingId)
        {
            StreamingId = resp.StreamingId;
            StreamingSw.Start();

            var first = ChatHelper.BuildChatLog(SessionCache, AppendedText, resp, StreamingSw);
            ChatCache.Add(resp.StreamingId, first);            
        }

        AppendedText += resp.Message.Text;
        var current = ChatHelper.BuildChatLog(SessionCache, AppendedText, resp, StreamingSw);
        ChatHelper.AppendChatLogs(StreamingId, ChatCache.ChatLogs, current);

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

        var id = Generator.NextStreamingId();
        ChatCache.Add(id, new()
        {
            ConnectionId = SessionCache.Session.ConnectionId,
            Username = SessionCache.Session.Username,
            Message = new(ChatSender.User, NewMessage),
            SentTime = DateTime.Now
        });

        var prev = ChatHelper.BuildPreviousMessages([.. ChatCache.ChatLogs.Values]);

        IsStreamingCompleted = false;
        Log($"[SENT] {SessionCache.Session.Username}: {NewMessage}");

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

        ChatCache.Clear();
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

