﻿namespace AiChatFrontend.Models;

public class FileChatRequest
{
    public byte[] FileStream { get; set; }
    public string? MediaType { get; set; }
    public ChatMsg Prompt { get; set; }
}
