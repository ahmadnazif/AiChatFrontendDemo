﻿using System.Security.AccessControl;

namespace AiChatFrontend.Models;

public class ChatHubChatResponse
{
    public string? Username { get; set; }
    public string? ConnectionId { get; set; }
    public string? RequestMessage { get; set; }
    public string? ResponseMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ModelId { get; set; }
}
