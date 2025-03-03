﻿using AiChatFrontend.EventArgs;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class StreamingChatBase : ComponentBase, IAsyncDisposable
{
    [Inject] public ILogger<StreamingChatBase> Logger { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    [Inject] public NavigationManager NavMan { get; set; }
    [Inject] public ChatService Chat { get; set; }
    [Inject] public ApiClient Api { get; set; }
    protected bool IsApiConnected { get; set; }
    protected bool IsChatting { get; set; } = false;
    protected bool IsStreaming { get; set; } = false;
    protected bool IsWaitingResponse { get; set; } = false;
    protected string? ResponseText { get; set; }
    protected string Username { get; set; }
    protected UserSession UserSession { get; set; }
    protected string ConnectionId { get; set; }
    protected string NewMessage { get; set; }
    protected List<ChatLog> ChatLogs { get; set; } = [];

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

    private void OnStreamingChatReceived(object sender, StreamingChatReceivedEventArgs e)
    {
        ResponseText += e.Response.Text;
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

            await Chat.StartChatStreamingAsync(NewMessage, prev);

            Log($"[SENT] {UserSession.Username}: {NewMessage}");

            NewMessage = string.Empty;
            IsWaitingResponse = true;
            IsStreaming = true;
        }
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

