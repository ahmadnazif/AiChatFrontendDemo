﻿using System.Security.AccessControl;

namespace AiChatFrontend.Models;

public class OneChatResponse
{
    public string? Username { get; set; }
    public string? ConnectionId { get; set; }
    public ChatMsg RequestMessage { get; set; }
    public ChatMsg ResponseMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ModelId { get; set; }
}
