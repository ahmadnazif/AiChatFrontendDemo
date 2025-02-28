using Microsoft.AspNetCore.SignalR.Client;

namespace AiChatFrontend.Helpers;

public static class HubHelper
{
    public static HubConnection CreateHubConnection(IConfiguration config, string hubRoute, string username, bool autoReconnect = true)
    {
        var hubUrl = $"{config["ApiUrl"].TrimEnd('/')}{hubRoute}?username={username}";
        var builder = new HubConnectionBuilder().WithUrl(hubUrl).AddMessagePackProtocol();

        if (autoReconnect)
            builder.WithAutomaticReconnect();

        return builder.Build();
    }
}
