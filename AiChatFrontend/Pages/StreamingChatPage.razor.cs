﻿using AiChatFrontend.EventArgs;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;
using System.Diagnostics;

namespace AiChatFrontend.Pages;

public class StreamingChatPageBase : ComponentBase, IAsyncDisposable
{
    [Inject] public ILogger<StreamingChatPageBase> Logger { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public NavigationManager NavMan { get; set; }
    [Inject] public ChatService Chat { get; set; }
    [Inject] public ApiClient Api { get; set; }
    protected bool IsApiConnected { get; set; }
    protected bool IsChatting { get; set; } = false;
    protected bool IsStreamingCompleted { get; set; } = true;
    protected string StreamingId { get; set; }
    protected Stopwatch StreamingSw { get; set; } = new Stopwatch();
    protected string? AppendedText { get; set; }
    protected string Username { get; set; }
    protected UserSession UserSession { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }
    protected Dictionary<string, ChatLog> ChatLogs { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        IsApiConnected = await Api.IsConnectedAsync();
    }

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

            Chat.OnStreamingChatReceived += OnStreamingChatReceived;

            IsChatting = true;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, true);
        }
    }

    //private void OnStreamingChatReceived(object sender, StreamingChatReceivedEventArgs e)
    //{
    //    var resp = e.Response;
    //    AppendedText += resp.Message.Text;

    //    if (resp.HasFinished)
    //    {
    //        var last = ChatHelper.BuildLastChatLog(ConnectionId, Username, AppendedText, resp);
    //        ChatLogs.Add(last);
    //        AppendedText = string.Empty;
    //        IsWaitingResponse = false;
    //        Toastr.Info($"Finished at {DateTime.Now.ToLongTimeString()}");
    //    }

    //    StateHasChanged();
    //}

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
        var last = ChatHelper.BuildChatLog(ConnectionId, Username, AppendedText, resp, StreamingSw);
        if (ChatLogs.TryGetValue(StreamingId, out var _))
        {
            ChatLogs[StreamingId] = last;
        }

        if (resp.HasFinished)
        {
            StreamingSw.Stop();
            StreamingSw.Reset();
            AppendedText = string.Empty;
            IsStreamingCompleted = true;
            Toastr.Info($"Finished at {DateTime.Now.ToLongTimeString()}");
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

        var id = Guid.NewGuid().ToString("N").ToUpper();
        ChatLogs.Add(id, new()
        {
            ConnectionId = UserSession.ConnectionId,
            Username = UserSession.Username,
            Message = new(ChatSender.User, NewMessage),
            SentTime = DateTime.Now
        });

        var prev = ChatHelper.BuildPreviousMessages([.. ChatLogs.Values]);

        IsStreamingCompleted = false;
        await Chat.StartChatStreamingAsync(NewMessage, prev);

        Log($"[SENT] {UserSession.Username}: {NewMessage}");

        NewMessage = string.Empty;
    }

    protected void Stop() => Chat.StopChatStreaming();

    protected async Task DisconnectAsync()
    {
        if (IsChatting)
        {
            Chat.OnStreamingChatReceived -= OnStreamingChatReceived;
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

