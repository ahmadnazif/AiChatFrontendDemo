﻿namespace AiChatFrontend.Models;

public class ChainedChatRequest
{
    public List<ChatMsg> PreviousMessages { get; set; }
    public ChatMsg Prompt { get; set; }
    public string? ModelId { get; set; }
}
